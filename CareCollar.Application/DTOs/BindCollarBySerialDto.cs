namespace CareCollar.Application.DTOs;

public class BindCollarBySerialDto
{
    public required Guid PetId { get; set; }

    public required string SerialNumber { get; set; }
}
