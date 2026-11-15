using System.ComponentModel.DataAnnotations;

namespace quizShared.DTOs;

public class ErrorDTO
{
    [Required]
    public required string Message { get; set; }
}
