using quizShared.DTOs;

namespace quizUi.Services;

public class QuizStateService
{
    private QuizDTO? _current;

    public void SetCurrentQuiz(QuizDTO quiz) => _current = quiz;

    /// <summary>
    /// Fetches the current User's Scoped Quiz and nullifies the state
    /// </summary>
    /// <returns>QuizDTO? _current</returns>
    public QuizDTO? ConsumeCurrentQuiz() { 
        var tmp = _current;
        _current = null;
        return tmp; 
    }
}