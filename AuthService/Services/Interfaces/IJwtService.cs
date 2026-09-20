using AuthService.Entities;

namespace AuthService.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}