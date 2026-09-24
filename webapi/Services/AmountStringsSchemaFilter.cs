using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace STX.Sdk.Api.Services
{
    /// <summary>
    /// Documents every field <see cref="AmountStringsJsonModifier"/> writes as a string as
    /// type string, so the OpenAPI document matches the responses.
    /// </summary>
    public class AmountStringsSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties is null || schema.Properties.Count == 0)
            {
                return;
            }

            foreach (var raw in context.Type.GetProperties())
            {
                if (AmountStringsJsonModifier.StringReaderFor(context.Type, raw) is null)
                {
                    continue;
                }

                // Schema keys are the camelCase JSON names.
                var key = schema.Properties.Keys
                    .FirstOrDefault(k => string.Equals(k, raw.Name, StringComparison.OrdinalIgnoreCase));
                if (key is null)
                {
                    continue;
                }

                schema.Properties[key] = new OpenApiSchema
                {
                    Type = "string",
                    Nullable = true,
                    Description = schema.Properties[key].Description,
                };
            }
        }
    }
}
