namespace CareCollar.Domain.Entities;

public class CollarDevice
{
    public Guid Id { get; set; }
    
    public Guid PetId { get; set; } 
    
    public required string SerialNumber { get; set; } 
    
    public DateTime LastConnection { get; set; } 
    
    public double BatteryLevel { get; set; }
    
    public Pet Pet { get; set; } = null!;
}