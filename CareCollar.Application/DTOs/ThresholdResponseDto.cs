using CareCollar.Domain.Enums;

namespace CareCollar.Application.DTOs;

public class ThresholdResponseDto
{
    public Guid Id { get; set; }
    
    public Guid PetId { get; set; }
    
    public MetricType MetricType { get; set; }
    
    public double? MinValue { get; set; }
    
    public double? MaxValue { get; set; }
    
    public string ThresholdName { get; set; }
}