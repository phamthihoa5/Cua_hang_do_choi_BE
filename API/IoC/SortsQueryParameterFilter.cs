using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.IoC
{
    public class SortsQueryParameterFilter : IParameterFilter
    {
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            if (parameter.Name == "Sorts")
            {
                parameter.Schema.Type = "array";
                parameter.Schema.Items = new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                {
                    { "key", new OpenApiSchema { Type = "string" } },
                    { "sort", new OpenApiSchema { Type = "integer" } }
                }
                };
            }
        }
    }

}
