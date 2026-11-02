namespace quizShared.DTOs;

public class FullAnswerDTO
{
    public required string Id {get; set;}
    public required string Text { get; set; }
    public required bool IsCorrect { get; set; }
}