using System.ComponentModel.DataAnnotations;

namespace quizUi.Options;

public class QuizApiOptions
{
    public const string SectionName = "QuizApiOptions";

    [Required]
    public string BaseUrl {get; set;} = string.Empty;
}