using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Products.Application.DTOs;
using Products.Application.Interfaces;

namespace Products.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _service.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var product = await _service.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        if (!TryGetCurrentUserFullName(out var fullName))
            return Unauthorized("Token must include name and surname claims.");

        var product = await _service.CreateAsync(dto, fullName);
        return Ok(product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductDto dto)
    {
        if (!TryGetCurrentUserFullName(out var fullName))
            return Unauthorized("Token must include name and surname claims.");

        var product = await _service.UpdateAsync(id, dto, fullName);
        return Ok(product);
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