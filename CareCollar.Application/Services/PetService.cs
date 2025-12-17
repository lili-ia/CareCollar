using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.Application.Mappers;
using CareCollar.Domain.Entities;
using CareCollar.Persistence;
using CareCollar.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareCollar.Application.Services;

public class PetService : IPetService
{
    private readonly CareCollarDbContext _context;
    private readonly ILogger<PetService> _logger;

    public PetService(ILogger<PetService> logger, CareCollarDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<Result<PetDto>> AddPetAsync(CreatePetDto petDto, Guid userId, CancellationToken ct)
    {
        var pet = new Pet
        {
            UserId = userId,
            Name = petDto.Name,
            Species = petDto.Species,
            WeightKg = petDto.WeightKg,
            Breed = petDto.Breed,
            DateOfBirth = DateTime.SpecifyKind(petDto.DateOfBirth, DateTimeKind.Utc) 
        };
        try
        {
            await _context.AddAsync(pet, ct);
            await _context.SaveChangesAsync(ct);

            var dto = pet.ToDto();
            return Result<PetDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding pet.");
            return Result<PetDto>.InternalServerError();
        }
    }
    
    public async Task<Result<PetDto>> GetPetByIdAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var petDto = await _context.Pets
            .AsNoTracking()
            .Where(p => p.Id == id && p.UserId == userId)
            .Select(PetMapper.ScalarProjection)
            .FirstOrDefaultAsync(ct);
        
        if (petDto is null)
            return Result<PetDto>.Failure("Pet not found", ErrorType.NotFound);
        
        return Result<PetDto>.Success(petDto);
    }

    public async Task<Result<List<PetDto>>> GetAllPetsAsync(Guid userId, CancellationToken ct)
    {
        var petDtos = await _context.Pets
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(PetMapper.ScalarProjection)
            .ToListAsync(ct);

        return Result<List<PetDto>>.Success(petDtos);
    }

    public async Task<Result<PetDto>> UpdatePetAsync(Guid id, UpdatePetDto petDto, Guid userId, CancellationToken ct)
    {
        var pet = await _context.Pets
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, ct);

        if (pet is null)
        {
            _logger.LogWarning("Attempted to update non-existent pet with ID: {PetId}", id);
            return Result<PetDto>.Failure("Pet not found.", ErrorType.NotFound);
        }

        pet.Name = petDto.Name;
        pet.Breed = petDto.Breed;
        pet.WeightKg = petDto.WeightKg;
        pet.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync(ct);

            var dto = pet.ToDto();
            return Result<PetDto>.Success(dto);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to save updates for pet {PetId}", id);
            return Result<PetDto>.InternalServerError();
        }
    }
}