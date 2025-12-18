namespace CareCollar.Application.DTOs;

public class NotificationDto
{
    public required string Title { get; set; }
    
    public required string Message { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}