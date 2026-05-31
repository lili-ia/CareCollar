namespace CareCollar.Application.DTOs;

public class TokenResponse
{
    public string Token { get; set; }
    public string Email { get; set; }
    public bool IsAdmin { get; set; }
}