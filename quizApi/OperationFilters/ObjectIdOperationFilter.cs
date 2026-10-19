using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace quizApi.OperationFilters;

public class ObjectIdOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var idParam = operation.Parameters?
            .FirstOrDefault(p => p.Name.Equals("id", StringComparison.OrdinalIgnoreCase)
                            && p.In == ParameterLocation.Path);

        // First cast to the concrete type, then assign
        if (idParam is OpenApiParameter concreteParam)
        {
            concreteParam.Schema = new OpenApiSchema { Type = JsonSchemaType.String };
        }
    }
}