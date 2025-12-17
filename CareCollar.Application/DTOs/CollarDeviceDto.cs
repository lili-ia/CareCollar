namespace CareCollar.Application.DTOs;

public class CollarDeviceDto : BaseDto
{
    public Guid? PetId { get; set; } 
    
    public string SerialNumber { get; set; } 
    
    public DateTime? LastConnection { get; set; } 
    
    public double BatteryLevel { get; set; }
}