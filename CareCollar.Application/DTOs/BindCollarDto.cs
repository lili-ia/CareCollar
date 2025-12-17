namespace CareCollar.Application.DTOs;

public class BindCollarDto
{
    public required Guid PetId { get; set; }

    public required Guid CollarId { get; set; }
}