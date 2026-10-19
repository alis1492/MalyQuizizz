using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using quizShared;

namespace quizApi.Models;


[BsonIgnoreExtraElements]
public class Quiz
{
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    public required string Title { get; set; }
    public required string Description { get; set; }
    public ObjectId TeacherId { get; set; }
    [BsonRepresentation(BsonType.Int32)]
    public Subject Subject { get; set; } = Subject.Other;
    public QuizSettings Settings { get; set; } = new QuizSettings();
    // Questions are nullable so its possible to create a test
    // and then create questions one by one
    public List<Question> Questions { get; set; } = [];
}