using CareCollar.Application.DTOs;
using CareCollar.Domain.Entities;
using CareCollar.Shared;

namespace CareCollar.Application.Contracts;

public interface IAuthService
{
    Task<Result<User>> RegisterAsync(string email, string password, CancellationToken ct);
    
    Task<Result<User>> ValidateUserAsync(string email, string password, CancellationToken ct);

    Task<Result> DeleteUserAsync(Guid userId, CancellationToken ct);

    Task<Result<List<UserDto>>> GetAllUsersAsync(Guid userId, CancellationToken ct);
    
    string GenerateJwtToken(User user);
}