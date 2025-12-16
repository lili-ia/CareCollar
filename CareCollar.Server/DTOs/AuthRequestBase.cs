namespace CareCollar.DTOs;

public class AuthRequestBase // TODO: add validation
{
    public required string Email { get; set; } 
    
    public required string Password { get; set; }
}