using CareCollar.Application.DTOs;
using CareCollar.Shared;

namespace CareCollar.Application.Contracts;

public interface IIngestionService
{
    Task<Result> ProcessDataAsync(HealthDataIngestionDto data, CancellationToken ct);
    
}