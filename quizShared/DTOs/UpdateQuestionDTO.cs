using System.ComponentModel.DataAnnotations;

namespace quizShared.DTOs;

public class UpdateQuestionDTO
{
    [Range(0, 1)]
    public int? Type { get; set; }
    public string? Text { get; set; }
    [Range(0.0001, 100)]
    public double? Points { get; set; }
    public List<UpdateAnswerDTO>? Answers { get; set; }
}