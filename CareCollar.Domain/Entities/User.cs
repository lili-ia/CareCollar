namespace CareCollar.Domain.Entities;

public class User : BaseEntity
{
    public required string Email { get; set; }
    
    public required string PasswordHash { get; set; } 
    
    public ICollection<Pet> Pets { get; set; } = [];
}