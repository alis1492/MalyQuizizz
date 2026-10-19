using System.ComponentModel.DataAnnotations;

namespace quizShared.DTOs;

public class CreateQuizDTO
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string TeacherId { get; set; }
    // Simple validation, 0 is always the first type in enum, Subject.Other is always the last
    [Range(
        0,
        (int)quizShared.Subject.Other
    )]
    public int Subject { get; set; } = (int)quizShared.Subject.Other;
    public QuizSettings Settings { get; set; } = new QuizSettings();
}