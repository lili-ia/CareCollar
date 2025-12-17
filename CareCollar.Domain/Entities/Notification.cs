namespace CareCollar.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    
    public required string Title { get; set; }
    
    public required string Message { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsRead { get; set; } = false;

    public User User { get; set; } = null!;
}