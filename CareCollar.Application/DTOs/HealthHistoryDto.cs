namespace CareCollar.Application.DTOs;

public record HealthHistoryDto(
    DateTime TimeBucket,
    double AvgHeartRate,
    double AvgTemperature
);