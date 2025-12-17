using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.Application.Mappers;
using CareCollar.Domain.Entities;
using CareCollar.Shared;
using Microsoft.EntityFrameworkCore;

namespace CareCollar.Application.Services;

public class ThresholdService(ICareCollarDbContext context)
    : IThresholdService
{
    public async Task<Result<ThresholdResponseDto>> CreateThresholdAsync(
        CreateThresholdDto req, 
        Guid userId, 
        CancellationToken ct)
    {
        var petExists = await context.Pets.AnyAsync(p => p.Id == req.PetId && p.UserId == userId, ct);
        if (!petExists) 
            return Result<ThresholdResponseDto>.Failure("Pet not found", ErrorType.NotFound);
        
        var threshold = new HealthThreshold
        {
            PetId = req.PetId,
            MetricType = req.MetricType,
            MinValue = req.MinValue,
            MaxValue = req.MaxValue,
            ThresholdName = req.ThresholdName
        };

        await context.HealthThresholds.AddAsync(threshold, ct);
        await context.SaveChangesAsync(ct);

        var dto = threshold.ToDto();
        
        return Result<ThresholdResponseDto>.Success(dto);
    }

    public async Task<Result> DeleteThresholdAsync(Guid thresholdId, Guid userId, CancellationToken ct)
    {
        var threshold = await context.HealthThresholds
            .FirstOrDefaultAsync(t => t.Id == thresholdId && t.Pet.UserId == userId, ct);

        if (threshold == null) 
            return Result.Failure("Threshold not found", ErrorType.NotFound);

        context.HealthThresholds.Remove(threshold);
        await context.SaveChangesAsync(ct);
        
        return Result.Success();
    }

    public async Task<Result<List<ThresholdResponseDto>>> GetPetThresholdsAsync(Guid petId, Guid userId, CancellationToken ct)
    {
        var thresholds = await context.HealthThresholds
            .AsNoTracking()
            .Where(t => t.PetId == petId && t.Pet.UserId == userId)
            .Select(ThresholdMapper.ProjectToDto)
            .ToListAsync(ct);

        return Result<List<ThresholdResponseDto>>.Success(thresholds);
    }
}