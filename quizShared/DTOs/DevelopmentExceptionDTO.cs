namespace quizShared.DTOs;

public class DevelopmentExceptionDTO
{
    public string Title { get; set; } = "Internal Error";
    public int Status {get; set;} = 500;
    public string Message {get; set;} = "Something went wrong";
    public string? StackTrace {get;set;}
    public string Timestamp {get; set;} = DateTime.UtcNow.ToString("o");

    public DevelopmentExceptionDTO() {}
    public DevelopmentExceptionDTO(string title, int status, Exception exception)
    {
        Title = title;
        Status = status;
        Message = exception.Message;
        StackTrace = exception.StackTrace;
    }
}