using System.Security.Claims;
using Contacts.Application.DTOs;
using Contacts.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contacts.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContactsController : ControllerBase
{
    private readonly IContactService _service;

    public ContactsController(IContactService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var contacts = await _service.GetAllAsync();
        return Ok(contacts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var contact = await _service.GetByIdAsync(id);
        if (contact == null)
            return NotFound();

        return Ok(contact);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateContactDto dto)
    {
        if (!TryGetCurrentUserFullName(out var fullName))
            return Unauthorized("Token must include name and surname claims.");

        var contact = await _service.CreateAsync(dto, fullName);
        return Ok(contact);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateContactDto dto)
    {
        if (!TryGetCurrentUserFullName(out var fullName))
            return Unauthorized("Token must include name and surname claims.");

        var contact = await _service.UpdateAsync(id, dto, fullName);
        return Ok(contact);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!TryGetCurrentUserFullName(out var fullName))
            return Unauthorized("Token must include name and surname claims.");

        await _service.DeleteAsync(id, fullName);
        return NoContent();
    }

    private bool TryGetCurrentUserFullName(out string fullName)
    {
        var name = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("name");
        var surname = User.FindFirstValue(ClaimTypes.Surname) ?? User.FindFirstValue("family_name");
        fullName = $"{name} {surname}".Trim();

        return !string.IsNullOrWhiteSpace(fullName);
    }
}
