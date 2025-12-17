using CareCollar.Application.DTOs;
using CareCollar.Shared;

namespace CareCollar.Application.Contracts;

public interface IPetService
{
    Task<Result<PetDto>> AddPetAsync(CreatePetDto petDto, Guid userId, CancellationToken ct);

    Task<Result<PetDto>> GetPetByIdAsync(Guid id, Guid userId, CancellationToken ct);

    Task<Result<List<PetDto>>> GetAllPetsAsync(Guid userId, CancellationToken ct);

    Task<Result<PetDto>> UpdatePetAsync(Guid id, UpdatePetDto petDto, Guid userId, CancellationToken ct);
} // NOTE: MVP provides no remove pet logic