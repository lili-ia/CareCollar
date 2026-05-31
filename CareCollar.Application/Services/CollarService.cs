using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.Application.Mappers;
using CareCollar.Domain.Entities;
using CareCollar.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareCollar.Application.Services;

public class CollarService(ICareCollarDbContext context, ILogger<CollarService> logger)
    : ICollarService
{

    public async Task<Result> BindToPetAsync(BindCollarDto dto, Guid userId, CancellationToken ct)
    {
        var petExists = await context.Pets
            .AnyAsync(p => p.Id == dto.PetId && p.UserId == userId, ct);

        if (!petExists)
            return Result.Failure("Pet not found", ErrorType.NotFound);

        var collar = await context.CollarDevices
            .FirstOrDefaultAsync(c => c.Id == dto.CollarId, ct);

        if (collar is null)
            return Result.Failure("Collar not found", ErrorType.NotFound);

        if (collar.PetId is not null && collar.PetId != dto.PetId)
            return Result.Failure("Collar is already assigned to another pet", ErrorType.Conflict);

        collar.PetId = dto.PetId;
        try
        {
            await context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to bind collar {CollarId} to pet {PetId}", dto.CollarId, dto.PetId);
            return Result.InternalServerError();
        }
    }

    public async Task<Result<Guid>> RegisterDeviceAsync(RegisterCollarDto dto, Guid userId, CancellationToken ct)
    {
        var isAdmin = await context.Users
            .AnyAsync(u => u.Id == userId && u.IsAdmin, ct);

        if (!isAdmin)
            return Result<Guid>.Failure("Access denied.", ErrorType.Forbidden);

        var device = new CollarDevice
        {
            SerialNumber = dto.SerialNumber
        };

        try
        {
            await context.CollarDevices.AddAsync(device, ct);
            await context.SaveChangesAsync(ct);
            return Result<Guid>.Success(device.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to register collar {SerialNumber}", dto.SerialNumber);
            return Result<Guid>.InternalServerError();
        }
    }

    public async Task<bool> UserOwnsCollarAsync(Guid collarId, Guid userId, CancellationToken ct)
    {
        return await context.CollarDevices
            .AsNoTracking()
            .AnyAsync(c => c.Id == collarId && c.Pet!.UserId == userId, ct);
    }

    public async Task<Result> BindToPetBySerialAsync(BindCollarBySerialDto dto, Guid userId, CancellationToken ct)
    {
        var petExists = await context.Pets
            .AnyAsync(p => p.Id == dto.PetId && p.UserId == userId, ct);

        if (!petExists)
            return Result.Failure("Pet not found", ErrorType.NotFound);

        var collar = await context.CollarDevices
            .FirstOrDefaultAsync(c => c.SerialNumber == dto.SerialNumber, ct);

        if (collar is null)
            return Result.Failure("Collar with this serial number not found", ErrorType.NotFound);

        if (collar.PetId is not null && collar.PetId != dto.PetId)
            return Result.Failure("Collar is already assigned to another pet", ErrorType.Conflict);

        collar.PetId = dto.PetId;
        try
        {
            await context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to bind collar {Serial} to pet {PetId}", dto.SerialNumber, dto.PetId);
            return Result.InternalServerError();
        }
    }

    public async Task<Result<List<CollarDeviceDto>>> GetPetCollarsAsync(Guid petId, Guid userId, CancellationToken ct)
    {
        var petExists = await context.Pets
            .AnyAsync(p => p.Id == petId && p.UserId == userId, ct);

        if (!petExists)
            return Result<List<CollarDeviceDto>>.Failure("Pet not found", ErrorType.NotFound);

        var collars = await context.CollarDevices
            .AsNoTracking()
            .Where(c => c.PetId == petId)
            .Select(CollarDeviceMapper.Projection)
            .ToListAsync(ct);

        return Result<List<CollarDeviceDto>>.Success(collars);
    }

    public async Task<Result<List<CollarDeviceDto>>> GetAllCollarsAsync(Guid adminId, CancellationToken ct)
    {
        var isAdmin = await context.Users
            .AnyAsync(u => u.Id == adminId && u.IsAdmin, ct);

        if (!isAdmin)
            return Result<List<CollarDeviceDto>>.Failure("Access denied.", ErrorType.Forbidden);

        var collars = await context.CollarDevices
            .AsNoTracking()
            .Select(CollarDeviceMapper.Projection)
            .ToListAsync(ct);

        return Result<List<CollarDeviceDto>>.Success(collars);
    }
}
