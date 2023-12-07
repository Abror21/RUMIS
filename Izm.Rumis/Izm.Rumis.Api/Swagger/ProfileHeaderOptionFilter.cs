using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Izm.Rumis.Api.Swagger
{
    public class ProfileHeaderOptionFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Profile",
                In = ParameterLocation.Header,
                Description = "Provide a valid user profile token.",
                Required = false
            });
        }
    }
}
