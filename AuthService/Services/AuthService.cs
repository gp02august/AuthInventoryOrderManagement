using AuthService.DTO;
using AuthService.Entities;
using AuthService.Repository.Interfaces;
using AuthService.Services.Interfaces;
using Microsoft.AspNetCore.Identity.Data;

namespace AuthService.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, IJwtService jwtService, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userRepository
            .GetByUsernameAsync(request.Username);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            Role = "USER",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.CreateAsync(user);

        _logger.LogInformation(
            "User {Username} registered successfully.",
            request.Username);

        return new RegisterResponseDto
        {
            UserId = createdUser.UserId,
            Username = createdUser.Username,
            Role = createdUser.Role,
            IsActive = createdUser.IsActive,
            CreatedAt = createdUser.CreatedAt
        };
    }
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository
            .GetByUsernameAsync(request.Username);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is inactive.");
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var accessToken = _jwtService.GenerateToken(user);

        _logger.LogInformation(
            "User {Username} logged in successfully.",
            request.Username);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };
    }
}