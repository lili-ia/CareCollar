namespace CareCollar.Application.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    public required string Message { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; }
}