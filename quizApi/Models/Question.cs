using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using quizShared;

namespace quizApi.Models;

[BsonIgnoreExtraElements]
public class Question
{
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    [BsonRepresentation(BsonType.Int32)]
    public required QuestionType Type { get; set; }
    public required string Text { get; set; }
    public required double Points { get; set; }
    public required List<Answer> Answers { get; set; }
}