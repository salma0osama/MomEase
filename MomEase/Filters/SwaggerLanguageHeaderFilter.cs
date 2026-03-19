using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
﻿using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MomEase.api.Filters
{
    public class SwaggerLanguageHeaderFilter : IOperationFilter
    {
        [AttributeUsage(AttributeTargets.Method)]
        public class LocalizedEndpointAttribute : Attribute { }
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasLocalizedAttribute = context.MethodInfo
               .GetCustomAttributes(typeof(LocalizedEndpointAttribute), false)
               .Any();

            // ⬅️ NEW: Skip if no attribute
            if (!hasLocalizedAttribute)
                return;
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Enum = new List<Microsoft.OpenApi.Any.IOpenApiAny>
                {
                    new Microsoft.OpenApi.Any.OpenApiString("en"),
                    new Microsoft.OpenApi.Any.OpenApiString("ar")
                },
                    Default = new Microsoft.OpenApi.Any.OpenApiString("en")
                },
                Description = "Language: en or ar"
            });
        }
    }
}
}
