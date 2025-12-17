using CareCollar.Application.DTOs;
using CareCollar.Shared;

namespace CareCollar.Application.Contracts;

public interface IThresholdService
{
    Task<Result<ThresholdResponseDto>> CreateThresholdAsync(CreateThresholdDto req, Guid userId, CancellationToken ct);

    Task<Result> DeleteThresholdAsync(Guid thresholdId, Guid userId, CancellationToken ct);

    Task<Result<List<ThresholdResponseDto>>> GetPetThresholdsAsync(Guid petId, Guid userId, CancellationToken ct);
}