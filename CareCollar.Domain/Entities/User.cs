namespace CareCollar.Domain.Entities;

public class User : BaseEntity
{
    public required string Email { get; set; }
    
    public required string PasswordHash { get; set; } 
    
    public bool IsAdmin { get; set; } = false;

    public string? FcmToken { get; set; }

    public ICollection<Pet> Pets { get; set; } = [];
}