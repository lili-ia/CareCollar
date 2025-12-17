using CareCollar.Application.DTOs;
using CareCollar.Shared;

namespace CareCollar.Application.Contracts;

public interface ICollarService
{
    Task<Result> BindToPetAsync(BindCollarDto dto, Guid userId, CancellationToken ct);

    Task<Result<Guid>> RegisterDeviceAsync(RegisterCollarDto dto, CancellationToken ct);
}