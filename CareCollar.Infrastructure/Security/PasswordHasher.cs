using CareCollar.Application.Contracts;

namespace CareCollar.Infrastructure.Security;

public class PasswordHasher() : IPasswordHasher
{
    public string HashPassword(string password) => 
        BCrypt.Net.BCrypt.HashPassword(password);
    
    public bool VerifyPassword(string password, string hashedPassword) => 
        BCrypt.Net.BCrypt.Verify(password, hashedPassword);
}