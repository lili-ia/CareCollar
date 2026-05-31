using CareCollar.Domain.Enums;

namespace CareCollar.DTOs;

public class ImportSettingsDto
{
    public List<ImportThresholdDto> Thresholds { get; set; } = [];
}

public class ImportThresholdDto
{
    public Guid PetId { get; set; }
    public MetricType MetricType { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public required string ThresholdName { get; set; }
}
