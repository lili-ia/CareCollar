using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.Domain.Entities;
using CareCollar.Persistence;
using CareCollar.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareCollar.Application.Services;

public class CollarService : ICollarService
{
    private readonly CareCollarDbContext _context;
    private readonly ILogger<CollarService> _logger;

    public CollarService(CareCollarDbContext context, ILogger<CollarService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> BindToPetAsync(BindCollarDto dto, Guid userId, CancellationToken ct)
    {
        var petExists = await _context.Pets
            .AnyAsync(p => p.Id == dto.PetId && p.UserId == userId, ct);

        if (!petExists)
            return Result.Failure("Pet not found", ErrorType.NotFound);

        var collar = await _context.CollarDevices
            .FirstOrDefaultAsync(c => c.Id == dto.CollarId, ct);

        if (collar is null)
            return Result.Failure("Collar not found", ErrorType.NotFound);

        if (collar.PetId is not null && collar.PetId != dto.PetId)
            return Result.Failure("Collar is already assigned to another pet", ErrorType.Conflict);

        collar.PetId = dto.PetId;
        try
        {
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to bind collar {CollarId} to pet {PetId}", dto.CollarId, dto.PetId);
            return Result.InternalServerError();
        }
    }

    public async Task<Result<Guid>> RegisterDeviceAsync(RegisterCollarDto dto, CancellationToken ct)
    {
        var device = new CollarDevice
        {
            SerialNumber = dto.SerialNumber
        };

        try
        {
            await _context.CollarDevices.AddAsync(device, ct);
            await _context.SaveChangesAsync(ct);
            return Result<Guid>.Success(device.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register collar {CollarSerialNumber}", dto.SerialNumber);
            return Result<Guid>.InternalServerError();
        }
    }
}