using System.Linq.Expressions;
using CareCollar.Application.DTOs;
using CareCollar.Domain.Entities;

namespace CareCollar.Application.Mappers;

public static class ThresholdMapper
{
    public static ThresholdResponseDto ToDto(this HealthThreshold threshold)
    {
        return new ThresholdResponseDto
        {
            Id = threshold.Id,
            PetId = threshold.PetId,
            MetricType = threshold.MetricType,
            MinValue = threshold.MinValue,
            MaxValue = threshold.MaxValue,
            ThresholdName = threshold.ThresholdName
        };
    }

    public static Expression<Func<HealthThreshold, ThresholdResponseDto>> ProjectToDto =>
        t => new ThresholdResponseDto
        {
            Id = t.Id,
            PetId = t.PetId,
            MetricType = t.MetricType,
            MinValue = t.MinValue,
            MaxValue = t.MaxValue,
            ThresholdName = t.ThresholdName
        };
}