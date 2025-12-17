using System.Linq.Expressions;
using CareCollar.Application.DTOs;
using CareCollar.Domain.Entities;

namespace CareCollar.Application.Mappers;

public static class CollarDeviceMapper
{
    public static CollarDeviceDto ToDto(this CollarDevice device) => new()
    {
        Id = device.Id,
        CreatedAt = device.CreatedAt,
        UpdatedAt = device.UpdatedAt,
        PetId = device.PetId,
        SerialNumber = device.SerialNumber,
        LastConnection = device.LastConnection,
        BatteryLevel = device.BatteryLevel
    };

    public static Expression<Func<CollarDevice, CollarDeviceDto>> Projection =>
        device => new CollarDeviceDto
        {
            Id = device.Id,
            PetId = device.PetId,
            SerialNumber = device.SerialNumber,
            LastConnection = device.LastConnection,
            BatteryLevel = device.BatteryLevel
        };
}