using System.Data;
using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using Dapper;

namespace CareCollar.Persistence.Repositories;

public class HealthDataRepository(IDbConnection dbConnection) : IHealthDataRepository // TODO: add global error handling
{
    public async Task<int> InsertHealthDataAsync(HealthDataIngestionDto data)
    {
        const string sql = @"
        INSERT INTO health_data (time, collar_id, heart_rate_bpm, temperature_celsius, gps_latitude, gps_longitude)
        SELECT 
            NOW(), 
            id, 
            @HeartRateBPM, 
            @TemperatureCelsius, 
            @Latitude, 
            @Longitude
        FROM collar_devices 
        WHERE serial_number = @SerialNumber;";

        var affectedRows = await dbConnection.ExecuteAsync(sql, data);
    
        if (affectedRows == 0)
        {
            throw new Exception($"Collar with serial number {data.SerialNumber} not found.");
        }

        return affectedRows;
    }
    
    public async Task<IEnumerable<HealthHistoryDto>> GetHistoryAsync(
        Guid collarId, 
        DateTime from, 
        DateTime to, 
        TimeSpan bucketInterval)
    {
        const string sql = @"
        SELECT 
            time_bucket(@Interval, time) AS TimeBucket,
            AVG(heart_rate_bpm) AS AvgHeartRate,
            AVG(temperature_celsius) AS AvgTemperature
        FROM health_data
        WHERE collar_id = @CollarId 
          AND time >= @From 
          AND time <= @To
        GROUP BY TimeBucket
        ORDER BY TimeBucket ASC;";

        var parameters = new
        {
            CollarId = collarId,
            From = from,
            To = to,
            Interval = bucketInterval 
        };

        return await dbConnection.QueryAsync<HealthHistoryDto>(sql, parameters);
    }
}