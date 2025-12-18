using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
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
            .AnyAsync(u => u.Id == userId && u.Email == "admin@example.com", ct);
        
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
            logger.LogError(ex, "Failed to register collar {CollarSerialNumber}", dto.SerialNumber);
            return Result<Guid>.InternalServerError();
        }
    }

    public async Task<bool> UserOwnsCollarAsync(Guid collarId, Guid userId, CancellationToken ct)
    {
        return await context.CollarDevices
            .AsNoTracking()
            .AnyAsync(c => c.Id == collarId && c.Pet!.UserId == userId, ct);
    }
}