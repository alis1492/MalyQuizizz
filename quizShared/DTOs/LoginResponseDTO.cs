using System.ComponentModel.DataAnnotations;

namespace quizShared.DTOs;

public class LoginResponseDTO
{
    [Required]
    public required string Token { get; set; }

    [Required]
    public required UserDTO User { get; set; }
}
