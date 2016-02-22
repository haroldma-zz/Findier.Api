using System.Data.Entity.Spatial;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Http;
using Findier.Api.Extensions;
using Microsoft.AspNet.Identity;

namespace Findier.Api.Infrastructure
{
    public abstract class BaseApiController : ApiController
    {
        private DbGeography _geoLocation;
        private string _ipAddress;
        private AppClaimsUser _user;

        public string ClientIpAddress => _ipAddress ?? (_ipAddress = Request.GetClientIpAddress());

        public DbGeography GeoLocation
        {
            get
            {
                if (_geoLocation != null)
                {
                    return _geoLocation;
                }

                _geoLocation = Request.GetGeoLocation();
                return _geoLocation;
            }
        }

        public bool IsAuthenticated => base.User?.Identity?.IsAuthenticated ?? false;

        public new AppClaimsUser User => _user ?? (IsAuthenticated ? (_user = new AppClaimsUser(base.User)) : null);

        public IHttpActionResult ApiBadRequest(string message)
        {
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, new { Error = message }));
        }

        public IHttpActionResult ApiBadRequestFromModelState()
        {
            var error = ModelState.FirstOrDefault().Value?.Errors?.FirstOrDefault()?.ErrorMessage;
            return error == null ? BadRequest() : ApiBadRequest(error);
        }

        public IHttpActionResult CreatedData(object data)
        {
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.Created, new { Data = data }));
        }

        public IHttpActionResult OkData(object data)
        {
            return Ok(new { Data = data });
        }

        public IHttpActionResult OkPageData(object data, bool hasNext)
        {
            return OkData(new { Results = data, HasNext = hasNext });
        }

        public IHttpActionResult OkPageData(object data, bool hasNext, bool hasPrev)
        {
            return OkData(new { Results = data, HasNext = hasNext, HasPrev = hasPrev });
        }
    }

    public class AppClaimsUser : ClaimsPrincipal
    {
        public AppClaimsUser(IPrincipal principal) : base(principal)
        {
            Load();
        }

        public AppClaimsUser(IIdentity identity) : base(identity)
        {
            Load();
        }

        public string EncodedId { get; set; }

        public int Id { get; set; }

        public string Username => Identity.Name;

        private void Load()
        {
            EncodedId = Identity.GetUserId();
            Id = Identity.GetUserIdDecoded();
        }
    }
}