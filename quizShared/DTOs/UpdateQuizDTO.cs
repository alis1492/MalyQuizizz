using System.ComponentModel.DataAnnotations;
using quizShared;

namespace quizShared.DTOs;

public class UpdateQuizDTO
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    // Simple validation, 0 is always the first type in enum, Subject.Other is always the last
    [Range(
        0,
        (int)quizShared.Subject.Other
    )]
    public int? Subject { get; set; }
    public QuizSettings? Settings { get; set; }
}