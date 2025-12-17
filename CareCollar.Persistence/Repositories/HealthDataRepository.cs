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
            VALUES (NOW(), @CollarId, @HeartRateBPM, @TemperatureCelsius, @Latitude, @Longitude);";

        return await dbConnection.ExecuteAsync(sql, data);
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