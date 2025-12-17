using System.Linq.Expressions;
using CareCollar.Application.DTOs;
using CareCollar.Domain.Entities;

namespace CareCollar.Application.Mappers;

public static class PetMapper
{
    public static PetDto ToDto(this Pet pet) => new()
    {
        Id = pet.Id,
        CreatedAt = pet.CreatedAt,
        UpdatedAt = pet.UpdatedAt,
        Name = pet.Name,
        Species = pet.Species,
        WeightKg = pet.WeightKg,
        Breed = pet.Breed,
        DateOfBirth = pet.DateOfBirth,
    };
    
    public static Expression<Func<Pet, PetDto>> ScalarProjection =>
        pet => new PetDto
        {
            Id = pet.Id,
            CreatedAt = pet.CreatedAt,
            UpdatedAt = pet.UpdatedAt,
            Name = pet.Name,
            Species = pet.Species,
            WeightKg = pet.WeightKg,
            Breed = pet.Breed,
            DateOfBirth = pet.DateOfBirth
        };
}