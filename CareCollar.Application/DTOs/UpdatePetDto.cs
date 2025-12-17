namespace CareCollar.Application.DTOs;

public class UpdatePetDto
{
    public string Name { get; set; }
    
    public double WeightKg { get; set; }
    
    public string? Breed { get; set; }
}