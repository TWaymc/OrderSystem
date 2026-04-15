namespace Auth.Application.DTOs;

public class AuthResponse
{
    public string Token { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Surame { get; set; } = default!;
}