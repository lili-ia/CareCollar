using CareCollar.Application.DTOs;
using CareCollar.Shared;

namespace CareCollar.Application.Contracts;

public interface IIngestionService
{
    Task<Result> ProcessDataAsync(HealthDataIngestionDto data, Guid userId, CancellationToken ct);
}