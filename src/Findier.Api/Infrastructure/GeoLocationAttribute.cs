using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Findier.Api.Extensions;

namespace Findier.Api.Infrastructure
{
    public class GeoLocationAttribute : ActionFilterAttribute
    {
        public override Task OnActionExecutingAsync(
            HttpActionContext actionContext,
            CancellationToken cancellationToken)
        {
            var geoLocation = actionContext.Request.GetGeoLocation();
            if (geoLocation == null)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest,
                    new { Error = "Missing a valid geo-location." });
            }
            return Task.FromResult<object>(null);
        }
    }
}