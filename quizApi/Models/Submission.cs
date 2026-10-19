using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using quizShared;

namespace quizApi.Models;

[BsonIgnoreExtraElements]
public class Submission
{
    public ObjectId Id {get; set;} = ObjectId.GenerateNewId();
    public required ObjectId QuizId {get; set;}
    [BsonRepresentation(BsonType.Int32)]
    public required UserType UserType {get; set;}
    public required ObjectId UserId {get; set;}
    // Utc in name so that future me doesnt accidentally use DateTime.Now accidentally
    public DateTime StartedAtUtc {get;set;} = DateTime.UtcNow;
    public DateTime? SubmittedAtUtc {get; set;}
    public Score Score {get; set;} = new Score();
    public List<UserAnswer> UserAnswers {get; set;} = [];
    [BsonRepresentation(BsonType.Int32)]
    public SubmissionStatus Status {get; set;} = SubmissionStatus.InProgress;
}