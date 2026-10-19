using System.ComponentModel.DataAnnotations;

namespace quizShared.DTOs;

public class CreateSubmissionDTO
{
    public required string QuizId {get; set;}
    [Range(0, (int)quizShared.UserType.Admin)]
    public required int UserType {get; set;}
    public required string UserId {get; set;}
}