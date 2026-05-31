using System.Net.Mime;
using System.Text;
using System.Text.Json;
using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.Domain.Entities;
using CareCollar.DTOs;
using CareCollar.Extensions;
using CareCollar.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareCollar.Controllers;

[Authorize]
[ApiController]
[Route("api/admin")]
[Produces(MediaTypeNames.Application.Json)]
public class AdminController(
    CareCollarDbContext dbContext,
    IUserContext userContext) : ControllerBase
{
    private async Task<bool> IsAdminAsync(CancellationToken ct) =>
        await dbContext.Users.AnyAsync(u => u.Id == userContext.UserId && u.IsAdmin, ct);

    /// <summary>[Admin] Exports all system data as JSON.</summary>
    [HttpGet("export/json")]
    public async Task<IActionResult> ExportJson(CancellationToken ct)
    {
        if (!await IsAdminAsync(ct)) return Forbid();

        var users = await dbContext.Users
            .Select(u => new { u.Id, u.Email, u.CreatedAt })
            .ToListAsync(ct);

        var pets = await dbContext.Pets
            .Select(p => new { p.Id, p.UserId, p.Name, p.Species, p.Breed, p.WeightKg, p.DateOfBirth, p.CreatedAt })
            .ToListAsync(ct);

        var collars = await dbContext.CollarDevices
            .Select(c => new { c.Id, c.SerialNumber, c.PetId, c.BatteryLevel, c.LastConnection, c.CreatedAt })
            .ToListAsync(ct);

        var thresholds = await dbContext.HealthThresholds
            .Select(t => new { t.Id, t.PetId, t.MetricType, t.ThresholdName, t.MinValue, t.MaxValue })
            .ToListAsync(ct);

        var export = new { ExportedAt = DateTime.UtcNow, users, pets, collars, thresholds };
        var json = JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });
        var bytes = Encoding.UTF8.GetBytes(json);

        return File(bytes, "application/json", $"carecollar-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
    }

    /// <summary>[Admin] Exports users as CSV.</summary>
    [HttpGet("export/users/csv")]
    public async Task<IActionResult> ExportUsersCsv(CancellationToken ct)
    {
        if (!await IsAdminAsync(ct)) return Forbid();

        var users = await dbContext.Users
            .Select(u => new { u.Id, u.Email, u.CreatedAt })
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("Id,Email,CreatedAt");
        foreach (var u in users)
            sb.AppendLine($"{u.Id},{u.Email},{u.CreatedAt:O}");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"users-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    /// <summary>[Admin] Exports pets as CSV.</summary>
    [HttpGet("export/pets/csv")]
    public async Task<IActionResult> ExportPetsCsv(CancellationToken ct)
    {
        if (!await IsAdminAsync(ct)) return Forbid();

        var pets = await dbContext.Pets
            .Select(p => new { p.Id, p.Name, p.Species, p.Breed, p.WeightKg, p.DateOfBirth, p.UserId })
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("Id,Name,Species,Breed,WeightKg,DateOfBirth,UserId");
        foreach (var p in pets)
            sb.AppendLine($"{p.Id},{p.Name},{p.Species},{p.Breed},{p.WeightKg},{p.DateOfBirth:O},{p.UserId}");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"pets-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    /// <summary>[Admin] Imports thresholds settings from JSON backup.</summary>
    [HttpPost("import/settings")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ImportSettings([FromBody] ImportSettingsDto dto, CancellationToken ct)
    {
        if (!await IsAdminAsync(ct)) return Forbid();

        if (dto?.Thresholds is null || dto.Thresholds.Count == 0)
            return BadRequest("No thresholds provided.");

        var imported = 0;
        foreach (var t in dto.Thresholds)
        {
            var petExists = await dbContext.Pets.AnyAsync(p => p.Id == t.PetId, ct);
            if (!petExists) continue;

            var existing = await dbContext.HealthThresholds
                .FirstOrDefaultAsync(h => h.PetId == t.PetId && h.MetricType == t.MetricType, ct);

            if (existing is not null)
            {
                existing.MinValue = t.MinValue;
                existing.MaxValue = t.MaxValue;
                existing.ThresholdName = t.ThresholdName;
            }
            else
            {
                dbContext.HealthThresholds.Add(new HealthThreshold
                {
                    PetId = t.PetId,
                    MetricType = t.MetricType,
                    MinValue = t.MinValue,
                    MaxValue = t.MaxValue,
                    ThresholdName = t.ThresholdName
                });
            }
            imported++;
        }

        await dbContext.SaveChangesAsync(ct);
        return Ok(new { Imported = imported });
    }
}
