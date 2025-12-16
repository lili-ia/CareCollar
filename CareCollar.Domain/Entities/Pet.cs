using CareCollar.Domain.Enums;

namespace CareCollar.Domain.Entities;

public class Pet : BaseEntity
{
    public Guid UserId { get; set; } 
    
    public required string Name { get; set; }
    
    public Species Species { get; set; } 
    
    public double WeightKg { get; set; }
    
    public string? Breed { get; set; }
    
    public DateTime DateOfBirth { get; set; }
    
    public User User { get; set; } = null!;
    
    public ICollection<CollarDevice> Devices { get; set; } = [];
    
    public ICollection<HealthThreshold> Thresholds { get; set; } = [];
}