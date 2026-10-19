namespace quizShared.DTOs;

public class QuizDTO
{
    public required string Id {get; set;}
    public required string Title { get; set; }
    public required string Description { get; set; }
    public Subject Subject { get; set; } = Subject.Other;
    public required QuizSettings Settings;
    // Questions are nullable so its possible to create a test
    // and then create questions one by one
    public List<QuestionDTO>? Questions;
}