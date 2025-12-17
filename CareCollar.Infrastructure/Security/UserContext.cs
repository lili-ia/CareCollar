using System.Security.Claims;
using CareCollar.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace CareCollar.Infrastructure.Security;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            return Guid.TryParse(userId, out var guid) ? guid : Guid.Empty;
        }
    }
}