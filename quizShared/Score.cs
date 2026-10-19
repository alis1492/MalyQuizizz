namespace quizShared;

// zeroed by default, populated after grading
public class Score
{
    public double TotalPoints {get;set;} = 0;
    public double MaxPoints {get; set;} = 0;
    public double Percentage {get; set;} = 0;
}