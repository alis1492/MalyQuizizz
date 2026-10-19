namespace quizShared.DTOs;

/// <summary>
/// FULL answer document, Question.Answers is fully replaced on update after validation
/// </summary>
public class UpdateAnswerDTO
{
    public required string Id { get; set; }
    public required string Text { get; set; }
    public required bool IsCorrect { get; set; }
}