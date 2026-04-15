using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Orders.Application.DTOs;
using Orders.Application.Interfaces;

namespace Orders.Infrastructure.HttpClients;

public class ContactApiClient : IContactApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ContactApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ContactDto?> GetByIdAsync(Guid id)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/contacts/{id}");
        ForwardAuthorizationHeader(request);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<ContactDto>();
    }

    private void ForwardAuthorizationHeader(HttpRequestMessage request)
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authHeader))
            request.Headers.TryAddWithoutValidation("Authorization", authHeader);
    }
}