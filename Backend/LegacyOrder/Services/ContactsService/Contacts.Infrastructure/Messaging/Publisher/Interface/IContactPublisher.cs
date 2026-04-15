namespace Contacts.Infrastructure.Messaging.Publisher.Interface;

public interface IContactPublisher
{
    Task PublishAsync(Guid contactId);
}
