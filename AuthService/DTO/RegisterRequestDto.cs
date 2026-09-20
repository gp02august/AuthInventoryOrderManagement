using System.ComponentModel.DataAnnotations;

namespace AuthService.DTO;

public class RegisterRequestDto
{
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}