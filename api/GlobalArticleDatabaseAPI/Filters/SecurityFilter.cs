using Core.Exceptions;
using GlobalArticleDatabase.DbContext.MongoDB.Interfaces;
using GlobalArticleDatabase.Helpers;
using GlobalArticleDatabase.Repositories.Interfaces;
using GlobalArticleDatabase.Services.Authentication.Implementations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace GlobalArticleDatabase.Filters
{
    public class SecurityFilter : Attribute, IAuthorizationFilter
    {
        IHttpContextAccessor _httpContext { get; }

        public SecurityFilter(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var endpointDescription = context.ActionDescriptor as ControllerActionDescriptor;

            // Skip if anonymous
            if (SecurityHelper.IsAnonymous(endpointDescription.ControllerName, endpointDescription.ActionName))
            {
                // if anonymous, we remove authorization header to prevent exceptions while processing token
                if (_httpContext.HttpContext.Request.Headers.ContainsKey(Constants.Authentication.AuthorizationHeaderKey))
                {
                    _httpContext.HttpContext.Request.Headers.Remove(Constants.Authentication.AuthorizationHeaderKey);
                }

                return;
            }

            var unathorizedException = new ExceptionBase(ExceptionCodes.IDENTITY_NOT_AUTHORIZED, "User is not authorized", null, StatusCodes.Status403Forbidden);

            // Retrieve the token and checks whether it´s a valid token according to token validation parameters (expiry date, issuer, audience, etc.)
            var token = JwtRetriever.GetUserToken(context.HttpContext);

            if (token == null) throw unathorizedException;

            // **********************************************************************
            // TODO: Improve performance of this section using Watch collections 
            //       in MongoDB or Redis to get the current valid tokens instead
            //       of direct access to the database.
            var claimsHelper = new ClaimsHelper(token.Claims);

            var renewRepository = Startup.GetService<IAuthRenewRepository>();

            var authRenew = renewRepository.GetByUserToken(token.RawData);

            // Check if the token exists in the database and it has not expired
            if (authRenew == null || authRenew.ExpiteAt <= DateTime.UtcNow) throw unathorizedException;
            // **********************************************************************

            // Check end point controller/action security based on Security.yaml description
            if (!SecurityHelper.CheckSecurity(endpointDescription.ControllerName, endpointDescription.ActionName, claimsHelper.Roles))
            {
                throw unathorizedException;
            }
        }
    }
}
