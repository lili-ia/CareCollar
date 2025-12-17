using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.Domain.Entities;
using CareCollar.Domain.Enums;
using CareCollar.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareCollar.Application.Services;

public class IngestionService(
    IHealthDataRepository dataRepo,
    ICareCollarDbContext efContext,
    ILogger<IngestionService> logger)
    : IIngestionService
{
    public async Task<Result> ProcessDataAsync(HealthDataIngestionDto data, CancellationToken ct)
    {
        await dataRepo.InsertHealthDataAsync(data);

        var thresholds = await efContext.HealthThresholds
            .AsNoTracking()
            .Where(t => t.Pet.Devices.Any(c => c.Id == data.CollarId))
            .ToListAsync(ct);

        foreach (var threshold in thresholds)
        {
            switch (threshold.MetricType)
            {
                case MetricType.HeartRate:
                    EvaluateThreshold(threshold, data.HeartRateBPM, "Heart Rate");
                    break;
                case MetricType.Temperature:
                    EvaluateThreshold(threshold, data.TemperatureCelsius, "Temperature");
                    break;
            }
        }

        return Result.Success();
    }

    private void EvaluateThreshold(HealthThreshold threshold, double currentValue, string metricName)
    {
        if (threshold.MaxValue.HasValue && currentValue > threshold.MaxValue.Value)
        {
            logger.LogCritical("ALERT: {Metric} too high! Threshold: {Name}, Value: {Value}", 
                metricName, threshold.ThresholdName, currentValue);
        }

        if (threshold.MinValue.HasValue && currentValue < threshold.MinValue.Value)
        {
            logger.LogCritical("ALERT: {Metric} too low! Threshold: {Name}, Value: {Value}", 
                metricName, threshold.ThresholdName, currentValue);
        }
    }
}