using MongoDB.Bson;

namespace quizApi.Models;

public class UserAnswer
{
    public required ObjectId QuestionId {get; set;}
    public List<ObjectId> SelectedAnswers {get; set;} = [];
    // set after grading, helpful for frontend team
    // if the quiz ever gets updated (which is unlikely due to the nature of simple quizes)
    // its fine since correct answers are written at the moment of graduation
    public List<ObjectId> CorrectAnswers {get; set;} = [];
    public double PointsAwarded {get; set;} = 0;
}