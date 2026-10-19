namespace quizShared.DTOs;

public class CreateAnswerDTO
{
    public required string Text { get; set; }
    public required bool IsCorrect { get; set; }
}