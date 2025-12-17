namespace CareCollar.Application.DTOs;

public class CollarResponseDto
{
    public Guid Id { get; set; }
    
    public string SerialNumber { get; set; } 
    
    public string Model { get; set; }
    
    public Guid? PetId { get; set; }
}