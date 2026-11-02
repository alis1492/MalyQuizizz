namespace quizShared.DTOs;

public class FullQuestionDTO
{
    public required string Id {get; set;}
    public required QuestionType Type { get; set; }
    public required string Text { get; set; }
    public required double Points { get; set; }
    public required List<FullAnswerDTO> Answers { get; set; }
}