using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.Domain.Entities;
using CareCollar.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CareCollar.Application.Services;

public class AuthService : IAuthService
{
    private readonly ICareCollarDbContext _context;
    private readonly byte[] _jwtSecret;
    private readonly ILogger<AuthService> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        ICareCollarDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _logger = logger;
        _passwordHasher = passwordHasher;

        var jwtSettings = configuration.GetSection("JwtSettings");
        _jwtSecret = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? string.Empty);
    }

    public async Task<Result<User>> RegisterAsync(string email, string password, CancellationToken ct)
    {
        var user = new User
        {
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(password)
        };

        try
        {
            await _context.Users.AddAsync(user, ct);
            await _context.SaveChangesAsync(ct);
            return Result<User>.Success(user);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
        {
            return Result<User>.Failure("User already exists", ErrorType.Conflict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user.");
            return Result<User>.InternalServerError();
        }
    }

    public async Task<Result<User>> ValidateUserAsync(string email, string password, CancellationToken ct)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user is null || !_passwordHasher.VerifyPassword(password, user.PasswordHash))
            return Result<User>.Failure("Invalid credentials", ErrorType.Unauthorized);

        return Result<User>.Success(user);
    }

    public async Task<Result> DeleteUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return Result.Failure("User not found", ErrorType.NotFound);

        try
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return Result.InternalServerError();
        }
    }

    public async Task<Result<List<UserDto>>> GetAllUsersAsync(Guid userId, CancellationToken ct)
    {
        var isAdmin = await _context.Users.AnyAsync(u => u.Id == userId && u.IsAdmin, ct);

        if (!isAdmin)
            return Result<List<UserDto>>.Failure("Access denied.", ErrorType.Forbidden);

        var users = await _context.Users
            .Select(u => new UserDto { Id = u.Id, Email = u.Email, CreatedAt = u.CreatedAt, IsAdmin = u.IsAdmin })
            .ToListAsync(ct);

        return Result<List<UserDto>>.Success(users);
    }

    public async Task<Result<UserDto>> GetCurrentUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return Result<UserDto>.Failure("User not found", ErrorType.NotFound);

        return Result<UserDto>.Success(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            IsAdmin = user.IsAdmin
        });
    }

    public async Task<Result> AdminDeleteUserAsync(Guid adminId, Guid targetUserId, CancellationToken ct)
    {
        var isAdmin = await _context.Users.AnyAsync(u => u.Id == adminId && u.IsAdmin, ct);

        if (!isAdmin)
            return Result.Failure("Access denied.", ErrorType.Forbidden);

        if (adminId == targetUserId)
            return Result.Failure("Cannot delete own account via admin endpoint.", ErrorType.Validation);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId, ct);

        if (user is null)
            return Result.Failure("User not found", ErrorType.NotFound);

        try
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin {AdminId} failed to delete user {UserId}", adminId, targetUserId);
            return Result.InternalServerError();
        }
    }

    public async Task<Result> SetAdminStatusAsync(Guid adminId, Guid targetUserId, bool isAdmin, CancellationToken ct)
    {
        var callerIsAdmin = await _context.Users.AnyAsync(u => u.Id == adminId && u.IsAdmin, ct);

        if (!callerIsAdmin)
            return Result.Failure("Access denied.", ErrorType.Forbidden);

        if (adminId == targetUserId && !isAdmin)
            return Result.Failure("Cannot revoke your own admin status.", ErrorType.Validation);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId, ct);

        if (user is null)
            return Result.Failure("User not found", ErrorType.NotFound);

        user.IsAdmin = isAdmin;
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    public TokenResponse GenerateTokenResponse(User user)
    {
        return new TokenResponse
        {
            Token = GenerateJwtToken(user),
            Email = user.Email,
            IsAdmin = user.IsAdmin
        };
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email)
        };

        if (user.IsAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_jwtSecret), SecurityAlgorithms.HmacSha256Signature)
        };

        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}
