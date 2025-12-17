namespace CareCollar.Application.DTOs;

public record HealthDataIngestionDto(
    Guid CollarId,
    double HeartRateBPM,
    double TemperatureCelsius,
    double Latitude,
    double Longitude
);