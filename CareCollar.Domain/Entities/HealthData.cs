namespace CareCollar.Domain.Entities;

public class HealthData
{
    public DateTime Time { get; set; } 
    
    public Guid CollarId { get; set; }
    
    public double HeartRateBPM { get; set; } 
    
    public double TemperatureCelsius { get; set; } 
    
    public double GpsLatitude { get; set; }
    
    public double GpsLongitude { get; set; }
    
    public double? ActivityIndex { get; set; } 
    
    public CollarDevice CollarDevice { get; set; } = null!;
}