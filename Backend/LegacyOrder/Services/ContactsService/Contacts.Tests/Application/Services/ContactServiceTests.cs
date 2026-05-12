using AutoMapper;
using Contacts.Application.DTOs;
using Contacts.Application.Interfaces;
using Contacts.Application.Mappings;
using Contacts.Application.Services;
using Contacts.Domain.Entities;
using Contacts.Domain.Interfaces;
using Contacts.Infrastructure.Messaging.Publisher.Interface;
using FluentAssertions;
using LoggingLib.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using RedisCache.Service;

namespace Contacts.Tests.Application.Services;

public class ContactServiceTests
{
    private readonly Mock<IContactRepository> _repo = new();
    private readonly Mock<ISequenceService> _sequences = new();
    private readonly Mock<ILogPublisher> _logger = new();
    private readonly Mock<IContactPublisher> _publisher = new();
    private readonly Mock<IRedisCacheService> _cache = new();
    private readonly IMapper _mapper;

    public ContactServiceTests()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ContactMappingProfile>(), loggerFactory);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistWithSequenceCodeAndMapToDto()
    {
        _sequences.Setup(x => x.GetNextContactCodeAsync(It.IsAny<CancellationToken>())).ReturnsAsync("C-00001");
        _repo.Setup(x => x.AddAsync(It.IsAny<Contact>())).ReturnsAsync((Contact c) =>
        {
            c.RowVersion = new byte[] { 1, 0, 0, 0 };
            c.CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return c;
        });

        var service = CreateService();
        var dto = new CreateContactDto("Ann", "Smith", null, "ann@example.com");

        var result = await service.CreateAsync(dto, "creator");

        result.Code.Should().Be("C-00001");
        result.Name.Should().Be("Ann");
        result.Surname.Should().Be("Smith");
        result.RowVersion.Should().NotBeNullOrEmpty();
        _repo.Verify(x => x.AddAsync(It.Is<Contact>(c => c.Code == "C-00001" && c.CreatedBy == "creator")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPassOriginalRowVersionToRepository()
    {
        var id = Guid.NewGuid();
        var token = Convert.ToBase64String(new byte[] { 9, 8, 7, 6 });
        var entity = new Contact
        {
            Id = id,
            Code = "C-00002",
            Name = "Old",
            Surname = "Name",
            RowVersion = new byte[] { 1, 1, 1, 1 }
        };

        _repo.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(entity);
        _repo.Setup(x => x.UpdateAsync(It.IsAny<Contact>(), It.IsAny<byte[]>()))
            .ReturnsAsync((Contact c, byte[] _) => c);

        var service = CreateService();
        await service.UpdateAsync(id, new UpdateContactDto("New", "Name", null, null, token), "editor");

        _repo.Verify(
            x => x.UpdateAsync(
                It.Is<Contact>(c => c.Name == "New" && c.LastModifiedBy == "editor"),
                It.Is<byte[]>(v => v.SequenceEqual(new byte[] { 9, 8, 7, 6 }))),
            Times.Once);
        _publisher.Verify(x => x.PublishAsync(id), Times.Once);
        _cache.Verify(x => x.RemoveAsync($"contacts:contact:{id}"), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCacheMiss_ShouldLoadFromRepositoryAndSetCache()
    {
        var id = Guid.NewGuid();
        var entity = new Contact
        {
            Id = id,
            Code = "C-00003",
            Name = "Bob",
            Surname = "Jones",
            RowVersion = new byte[] { 2, 0, 0, 0 },
            CreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedBy = "u",
            LastModifiedBy = "u"
        };

        _cache.Setup(x => x.GetAsync<ContactDto>($"contacts:contact:{id}")).ReturnsAsync((ContactDto?)null);
        _repo.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(entity);

        var service = CreateService();
        var result = await service.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        _cache.Verify(
            x => x.SetAsync($"contacts:contact:{id}", It.IsAny<ContactDto>(), TimeSpan.FromMinutes(30)),
            Times.Once);
    }

    private ContactService CreateService()
    {
        return new ContactService(
            _repo.Object,
            _sequences.Object,
            _mapper,
            _logger.Object,
            _publisher.Object,
            _cache.Object);
    }
}
