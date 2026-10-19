namespace quizShared;

public class QuizSettings
{
    public bool ShuffleQuestions { get; set; } = false;
    public bool ShuffleAnswers { get; set; } = true;
    public int? DefaultQuestionTimeLimit { get; set; }
    public int? TimeLimit { get; set; }
}