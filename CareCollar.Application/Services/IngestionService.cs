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
    ICareCollarDbContext context,
    ILogger<IngestionService> logger,
    INotificationService notificationService)
    : IIngestionService
{
    public async Task<Result> ProcessDataAsync(HealthDataIngestionDto data, CancellationToken ct)
    {
        await dataRepo.InsertHealthDataAsync(data);

        var thresholds = await context.HealthThresholds
            .AsNoTracking()
            .Where(t => t.Pet.Devices.Any(c => c.Id == data.CollarId))
            .ToListAsync(ct);

        foreach (var threshold in thresholds)
        {
            double? currentValue = threshold.MetricType switch
            {
                MetricType.HeartRate => data.HeartRateBPM,
                MetricType.Temperature => data.TemperatureCelsius,
                _ => null
            };

            if (!currentValue.HasValue || (!(currentValue > threshold.MaxValue) && !(currentValue < threshold.MinValue))) 
                continue;

            var pet = await context.Pets
                .AsNoTracking()
                .Where(p => p.Id == threshold.PetId)
                .Select(p => new Pet { UserId = p.UserId, Name = p.Name })
                .FirstOrDefaultAsync(ct);
            
            if (pet is not null)
            {
                await notificationService.CreateNotificationAsync(
                    pet.UserId, 
                    $"Abnormal {threshold.MetricType}", 
                    $"{pet.Name}'s {threshold.MetricType} is {currentValue}. Threshold name: {threshold.ThresholdName}",
                    ct
                );
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