using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using quizShared;

namespace quizApi.Models;

[BsonIgnoreExtraElements]
public class User
{
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }

    [BsonRepresentation(BsonType.Int32)]
    public UserType UserType { get; set; } = UserType.Student;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}