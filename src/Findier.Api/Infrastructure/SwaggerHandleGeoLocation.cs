using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Findier.Api.Infrastructure
{
    public class SwaggerHandleGeoLocation : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var fromHeaderAttributes =
                apiDescription.ActionDescriptor.GetCustomAttributes<GeoLocationAttribute>().Any();

            if (fromHeaderAttributes)
            {
                operation.parameters.Add(new Parameter
                {
                    name = "X-Geo-Location",
                    @in = "header",
                    type = "string",
                    @default = "18.4219332,-66.0766721",
                    required = true,
                    description = "The user's lat lon pair"
                });
            }
        }
    }
}