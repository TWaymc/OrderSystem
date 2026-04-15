using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.DTOs;
using Orders.Application.Interfaces;

namespace Orders.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _service.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var order = await _service.GetByIdAsync(id);
        if (order == null)
            return NotFound();

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        if (!TryGetCurrentUserFullName(out var fullName))
            return Unauthorized("Token must include name and surname claims.");

        var order = await _service.CreateAsync(dto, fullName);
        return Ok(order);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateOrderStatusDto dto)
    {
        if (!TryGetCurrentUserFullName(out var fullName))
            return Unauthorized("Token must include name and surname claims.");

        var order = await _service.UpdateStatusAsync(id, dto, fullName);
        return Ok(order);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateOrderDto dto)
    {
        if (!TryGetCurrentUserFullName(out var fullName))
            return Unauthorized("Token must include name and surname claims.");

        var order = await _service.UpdateAsync(id, dto, fullName);
        return Ok(order);
    }

    [HttpPost("{id}/items")]
    public async Task<IActionResult> AddOrderItem(Guid id, AddOrderItemDto dto)
    {
        if (!TryGetCurrentUserFullName(out var fullName))
            return Unauthorized("Token must include name and surname claims.");

        var order = await _service.AddOrderItemAsync(id, dto, fullName);
        return Ok(order);
    }

    [HttpDelete("{id}/items/{orderItemId}")]
    public async Task<IActionResult> RemoveOrderItem(Guid id, Guid orderItemId)
    {
        if (!TryGetCurrentUserFullName(out var fullName))
            return Unauthorized("Token must include name and surname claims.");

        var removed = await _service.RemoveOrderItemAsync(id, orderItemId, fullName);
        if (!removed)
            return NotFound();

        return NoContent();
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
