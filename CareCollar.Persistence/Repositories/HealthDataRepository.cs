using System.Data;
using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using Dapper;

namespace CareCollar.Persistence.Repositories;

public class HealthDataRepository(IDbConnection dbConnection) : IHealthDataRepository
{
    public async Task<int> InsertHealthDataAsync(HealthDataIngestionDto data)
    {
        const string sql = @"
        WITH target AS (
            UPDATE collar_devices
            SET battery_level = COALESCE(@BatteryLevel, battery_level),
                last_connection = NOW()
            WHERE serial_number = @SerialNumber
            RETURNING id
        )
        INSERT INTO health_data (time, collar_id, heart_rate_bpm, temperature_celsius, gps_latitude, gps_longitude)
        SELECT NOW(), id, @HeartRateBPM, @TemperatureCelsius, @Latitude, @Longitude
        FROM target;";

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
            to_timestamp(
                floor(extract(epoch from time) / @BucketSeconds) * @BucketSeconds
            ) AT TIME ZONE 'UTC' AS TimeBucket,
            AVG(heart_rate_bpm)      AS AvgHeartRate,
            AVG(temperature_celsius) AS AvgTemperature,
            AVG(gps_latitude)        AS AvgLatitude,
            AVG(gps_longitude)       AS AvgLongitude
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
            BucketSeconds = (long)bucketInterval.TotalSeconds
        };

        return await dbConnection.QueryAsync<HealthHistoryDto>(sql, parameters);
    }

    public async Task<LatestHealthDto?> GetLatestAsync(Guid collarId)
    {
        const string sql = @"
        SELECT
            time            AS Time,
            heart_rate_bpm  AS HeartRateBPM,
            temperature_celsius AS TemperatureCelsius,
            gps_latitude    AS GpsLatitude,
            gps_longitude   AS GpsLongitude,
            activity_index  AS ActivityIndex
        FROM health_data
        WHERE collar_id = @CollarId
        ORDER BY time DESC
        LIMIT 1;";

        return await dbConnection.QueryFirstOrDefaultAsync<LatestHealthDto>(sql, new { CollarId = collarId });
    }
}