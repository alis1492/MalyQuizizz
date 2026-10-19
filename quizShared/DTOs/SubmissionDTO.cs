namespace quizShared.DTOs;

public class SubmissionDTO 
{
    public required string Id {get; set;}
    public required string QuizId {get; set;}
    public required DateTime StartedAtUtc {get; set;}
    public DateTime? SubmittedAtUtc {get; set;}
    public Score Score {get; set;} = new Score();
    public List<UserAnswerDTO>? UserAnswers {get; set;}
    public required int Status {get; set;}
}