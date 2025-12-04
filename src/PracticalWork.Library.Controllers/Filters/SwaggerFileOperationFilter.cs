using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasFormFile = context.MethodInfo.GetParameters()
            .Any(p => p.ParameterType == typeof(IFormFile) || 
                      p.ParameterType.GetProperties().Any(x => x.PropertyType == typeof(IFormFile)));

        if (!hasFormFile) return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = context.MethodInfo.GetParameters()
                            .SelectMany(p => p.ParameterType.GetProperties())
                            .ToDictionary(
                                prop => prop.Name,
                                prop => new OpenApiSchema
                                {
                                    Type = prop.PropertyType == typeof(IFormFile) ? "string" : "string",
                                    Format = prop.PropertyType == typeof(IFormFile) ? "binary" : null
                                }
                            )
                    }
                }
            }
        };
    }
}