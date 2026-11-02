using System.ComponentModel.DataAnnotations;

namespace quizShared.DTOs;

public class RegisterUserDTO
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string FirstName { get; set; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string LastName { get; set; }

    [Required, EmailAddress, StringLength(200)]
    public required string Email { get; set; }

    [Required, StringLength(200, MinimumLength = 8)]
    public required string Password { get; set; }   // plaintext; hashed server-side

    [Range(0, (int)quizShared.UserType.Admin)]
    public int UserType { get; set; } = (int)quizShared.UserType.Student;
}