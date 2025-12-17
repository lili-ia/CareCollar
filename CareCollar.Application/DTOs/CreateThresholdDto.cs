using CareCollar.Domain.Enums;

namespace CareCollar.Application.DTOs;

public record CreateThresholdDto(
    Guid PetId,
    MetricType MetricType,
    double? MinValue,
    double? MaxValue,
    string ThresholdName
);