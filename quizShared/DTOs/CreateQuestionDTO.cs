using System.ComponentModel.DataAnnotations;

namespace quizShared.DTOs;

public class CreateQuestionDTO
{
    // hardcoded range based on Models.QuestionType
    // not scalable but works for mvp
    [Range(0, 1)]
    public required int Type { get; set; }
    public required string Text { get; set; }
    [Range(0.0001, 100)]
    public required double Points { get; set; }
    [Length(2, 10)]
    public required List<CreateAnswerDTO> Answers { get; set; }
}