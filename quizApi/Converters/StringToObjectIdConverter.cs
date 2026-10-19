using AutoMapper;
using MongoDB.Bson;

namespace quizApi.Converters;

public class StringToObjectIdConverter : ITypeConverter<string, ObjectId>
{
    public ObjectId Convert(string source, ObjectId destination, ResolutionContext context)
        => ObjectId.TryParse(source, out var id) ? id : ObjectId.Empty;
}