namespace CareCollar.Application.DTOs;

public record HealthDataIngestionDto(
    string SerialNumber,
    double HeartRateBPM,
    double TemperatureCelsius,
    double Latitude,
    double Longitude
);