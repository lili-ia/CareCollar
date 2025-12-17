using CareCollar.Application.DTOs;

namespace CareCollar.Application.Contracts;

public interface IHealthDataRepository
{
    Task<int> InsertHealthDataAsync(HealthDataIngestionDto data);
    
    Task<IEnumerable<HealthHistoryDto>> GetHistoryAsync(Guid collarId, DateTime from, DateTime to, TimeSpan bucketInterval);
}