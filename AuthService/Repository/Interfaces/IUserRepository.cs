using AuthService.Entities;

namespace AuthService.Repository.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);

    Task<User> CreateAsync(User user);
}