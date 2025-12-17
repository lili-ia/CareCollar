using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CareCollar.Application.Contracts;
using CareCollar.Domain.Entities;
using CareCollar.Persistence;
using CareCollar.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CareCollar.Application.Services;

public class AuthService : IAuthService
{
    private readonly CareCollarDbContext _context;
    private readonly byte[] _jwtSecret;
    private readonly ILogger<AuthService> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        CareCollarDbContext context, 
        IConfiguration configuration, 
        ILogger<AuthService> logger, 
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _logger = logger;
        _passwordHasher = passwordHasher;

        var jwtSettings = configuration.GetSection("JwtSettings");
        var secret = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? string.Empty);
        _jwtSecret = secret;
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
            _logger.LogError(ex, "Error registering user subscription.");
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
    
    public string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
                new Claim(ClaimTypes.Email, user.Email)
            ]),
            Expires = DateTime.UtcNow.AddDays(7), 
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_jwtSecret), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}