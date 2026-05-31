using CareCollar.Application.DTOs;
using CareCollar.Shared;

namespace CareCollar.Application.Contracts;

public interface ICollarService
{
    Task<Result> BindToPetAsync(BindCollarDto dto, Guid userId, CancellationToken ct);

    Task<Result> BindToPetBySerialAsync(BindCollarBySerialDto dto, Guid userId, CancellationToken ct);

    Task<Result<Guid>> RegisterDeviceAsync(RegisterCollarDto dto, Guid userId, CancellationToken ct);

    Task<bool> UserOwnsCollarAsync(Guid collarId, Guid userId, CancellationToken ct);

    Task<Result<List<CollarDeviceDto>>> GetPetCollarsAsync(Guid petId, Guid userId, CancellationToken ct);

    Task<Result<List<CollarDeviceDto>>> GetAllCollarsAsync(Guid adminId, CancellationToken ct);
}
