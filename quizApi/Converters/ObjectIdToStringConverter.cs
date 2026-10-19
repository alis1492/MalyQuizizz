using AutoMapper;
using MongoDB.Bson;

namespace quizApi.Converters;

public class ObjectIdToStringConverter : ITypeConverter<ObjectId, string>
{
    public string Convert(ObjectId source, string destination, ResolutionContext context)
    {
        return source.ToString();
    }
}