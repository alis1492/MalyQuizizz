using quizShared;

namespace quizShared.DTOs;

public class UserDTO
{
    public required string Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required UserType UserType { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
}