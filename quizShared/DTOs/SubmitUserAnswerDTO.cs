namespace quizShared.DTOs;

public class SubmitUserAnswerDTO
{
    public required string QuestionId {get; set;}
    public List<string> SelectedAnswers {get; set;} = [];   
}