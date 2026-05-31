namespace CareCollar.Application.DTOs;

public record LatestHealthDto(
    DateTime Time,
    double HeartRateBPM,
    double TemperatureCelsius,
    double GpsLatitude,
    double GpsLongitude,
    double? ActivityIndex
);
