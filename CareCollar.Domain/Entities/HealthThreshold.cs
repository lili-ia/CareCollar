using CareCollar.Domain.Enums;

namespace CareCollar.Domain.Entities;

public class HealthThreshold : BaseEntity
{
    public Guid PetId { get; set; } 
    
    public MetricType MetricType { get; set; }
    
    public double? MinValue { get; set; } 
    
    public double? MaxValue { get; set; } 
    
    public required string ThresholdName { get; set; } 
    
    public Pet Pet { get; set; } = null!;
}