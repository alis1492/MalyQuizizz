namespace quizShared.DTOs;

public class QuestionDTO
{
    public required string Id {get; set;}
    public required QuestionType Type { get; set; }
    public required string Text { get; set; }
    public required double Points { get; set; }
    public required List<AnswerDTO> Answers { get; set; }
}