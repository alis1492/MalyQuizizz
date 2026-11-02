namespace quizShared.DTOs;

public class FullQuizDTO
{
    public required string Id {get; set;}
    public required string Title { get; set; }
    public required string Description { get; set; }
    public Subject Subject { get; set; } = Subject.Other;
    public required QuizSettings Settings {get; set;}
    // Questions are nullable so its possible to create a test
    // and then create questions one by one
    public List<FullQuestionDTO> Questions {get; set;} = [];
}