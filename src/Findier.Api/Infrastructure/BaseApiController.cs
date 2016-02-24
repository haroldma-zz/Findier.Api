using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Web.Http;
using Findier.Api.Extensions;
using Findier.Api.Models;
using Findier.Api.Responses;
using Microsoft.AspNet.Identity;

namespace Findier.Api.Infrastructure
{
    public abstract class BaseApiController : ApiController
    {
        private DbGeography _geoLocation;
        private string _ipAddress;
        private AppClaimsUser _user;

        protected string ClientIpAddress => _ipAddress ?? (_ipAddress = Request.GetClientIpAddress());

        protected DbGeography GeoLocation
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

        protected bool IsAuthenticated => base.User?.Identity?.IsAuthenticated ?? false;

        protected new AppClaimsUser User => _user ?? (IsAuthenticated ? (_user = new AppClaimsUser(base.User)) : null);

        protected IHttpActionResult ApiBadRequest(string message)
        {
            return
                ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest,
                    new FindierErrorResponse { Error = message }));
        }

        protected IHttpActionResult ApiBadRequestFromModelState()
        {
            var error = ModelState.FirstOrDefault().Value?.Errors?.FirstOrDefault()?.ErrorMessage;
            return error == null ? BadRequest() : ApiBadRequest(error);
        }

        protected IHttpActionResult CreatedData<T>(T data)
        {
            return
                ResponseMessage(Request.CreateResponse(HttpStatusCode.Created,
                    new FindierResponse<T> { Data = data }));
        }

        protected IHttpActionResult NoContent()
        {
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
        }

        protected IHttpActionResult OkData<T>(T data)
        {
            return Ok(new FindierResponse<T> { Data = data });
        }

        protected IHttpActionResult OkPageData<T>(List<T> data, bool hasNext, bool hasPrev = false)
        {
            return OkData(new FindierPageData<T>
            {
                Results = data,
                HasNext = hasNext,
                HasPrev = hasPrev
            });
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