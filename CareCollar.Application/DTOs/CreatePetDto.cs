using CareCollar.Domain.Enums;

namespace CareCollar.Application.DTOs;

public class CreatePetDto // TODO add validation
{
    public required string Name { get; set; }
    
    public Species Species { get; set; } 
    
    public double WeightKg { get; set; }
    
    public string? Breed { get; set; }
    
    public DateTime DateOfBirth { get; set; }
}