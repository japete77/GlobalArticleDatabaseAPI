using Core.Exceptions;
using GlobalArticleDatabase.Helpers;
using GlobalArticleDatabase.Helpers.Framework.Helpers.Threads;
using GlobalArticleDatabase.Services.Authentication.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace GlobalArticleDatabase.Services.Authentication.Implementations
{
    public class JwtRetriever : IJwtRetriever
    {
        private readonly JwtSecurityToken _securityToken = null;

        private readonly List<string> _authEndPoints = new List<string>
        {
            "auth/login",
            "auth/forgot-password" ,
            "auth/reset-password"
        };

        public JwtRetriever(IHttpContextAccessor context)
        {
            if (context == null || context.HttpContext == null) return;

            var headers = context.HttpContext.Request.Headers;
            if (headers.ContainsKey(Constants.Authentication.AuthorizationHeaderKey))
            {
                _securityToken = GetUserToken(context.HttpContext);
            }
            else
            {
                // if login endpoint we retrieve the tenant from body payload
                if (context.HttpContext.Request.Method == "POST" &&
                    IsAnonymousInAuthenticationController(context.HttpContext.Request.Path.Value))
                {
                    // buffering must be enabled to rewind the body stream
                    context.HttpContext.Request.EnableBuffering();

                    // Leave the body open so the next middleware can read it.
                    using var reader = new StreamReader(
                        context.HttpContext.Request.Body,
                        encoding: Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: false,
                        bufferSize: Constants.App.StreamBufferSize,
                        leaveOpen: true);

                    var body = AsyncHelper.RunSync(() => reader.ReadToEndAsync());

                    // Retrieve payload object assuming properties Tenant and User
                    var payload = JsonConvert.DeserializeObject<dynamic>(body);
                    string tenant = payload.tenant;
                    string user = payload.user;

                    _securityToken = new JwtSecurityToken(
                        null,
                        null,
                        new List<Claim>()
                        {
                                new Claim(ClaimsHelper.TENANT_KEY, string.IsNullOrEmpty(tenant) ? Constants.App.MasterTenant : tenant),
                                new Claim(ClaimsHelper.TENANT_DB_NAME, string.IsNullOrEmpty(tenant) ? Constants.App.MasterTenant :tenant.NormalizeCharacters()),
                                new Claim(ClaimsHelper.USERNAME_KEY, string.IsNullOrEmpty(user) ? "Anonymous" : user)
                        });

                    // Reset the request body stream position so the next middleware can read it
                    context.HttpContext.Request.Body.Position = 0;
                }
            }
        }

        private bool IsAnonymousInAuthenticationController(string path)
        {
            foreach (var item in _authEndPoints)
            {
                if (path.ToLower().EndsWith(item)) return true;
            }

            return false;
        }

        public static JwtSecurityToken GetUserToken(HttpContext context)
        {
            var bearerHeader = context.Request.Headers[Constants.Authentication.AuthorizationHeaderKey].FirstOrDefault();

            // check if null value
            if (bearerHeader == null)
            {
                throw new AuthenticationException(ExceptionCodes.IDENTITY_AUTHORIZATION_TOKEN_NOT_FOUND, "Authorization token not found", null, StatusCodes.Status401Unauthorized);
            }

            var authToken = bearerHeader.Replace(Constants.Authentication.BearerType, "").Trim();

            var tokenHandler = new JwtSecurityTokenHandler();
            if (tokenHandler.CanReadToken(authToken))
            {
                try
                {
                    // Validate token
                    tokenHandler.ValidateToken(authToken, Constants.Authentication.tokenValidationParameters, out SecurityToken securityToken);

                    return securityToken as JwtSecurityToken;
                }
                catch (Exception innerException)
                {
                    if (innerException is SecurityTokenDecryptionFailedException)
                    {
                        throw new AuthenticationException(ExceptionCodes.IDENTITY_AUTHORIZATION_TOKEN_DECRYPTION_ERROR, "Authorization token decryption fails", innerException, StatusCodes.Status403Forbidden);
                    }
                    else if (innerException is SecurityTokenEncryptionKeyNotFoundException)
                    {
                        throw new AuthenticationException(ExceptionCodes.IDENTITY_AUTHORIZATION_TOKEN_ENCRYPTION_KEY_NOT_FOUND, "Security token contained a key identifier but the key was not found by the runtime when decrypting", innerException, StatusCodes.Status403Forbidden);
                    }
                    else if (innerException is SecurityTokenException)
                    {
                        throw new AuthenticationException(ExceptionCodes.IDENTITY_AUTHORIZATION_SECURITY_EXCEPTION, "Error processing security token", innerException, StatusCodes.Status403Forbidden);
                    }
                    else if (innerException is SecurityTokenExpiredException)
                    {
                        throw new AuthenticationException(ExceptionCodes.IDENTITY_AUTHORIZATION_TOKEN_EXPIRED, "Token has expired", innerException, StatusCodes.Status403Forbidden);
                    }
                    else if (innerException is SecurityTokenInvalidAudienceException)
                    {
                        throw new AuthenticationException(ExceptionCodes.IDENTITY_AUTHORIZATION_INVALID_AUDIENCE, "Audience of the token is not valid", innerException, StatusCodes.Status403Forbidden);
                    }
                    else if (innerException is SecurityTokenInvalidIssuerException)
                    {
                        throw new AuthenticationException(ExceptionCodes.IDENTITY_AUTHORIZATION_INVALID_ISSUER, "Issuer of the token is not valid", innerException, StatusCodes.Status403Forbidden);
                    }
                    else if (innerException is SecurityTokenInvalidLifetimeException)
                    {
                        throw new AuthenticationException(ExceptionCodes.IDENTITY_AUTHORIZATION_INVALID_LIFETIME, "Lifetime of the token is not valid", innerException, StatusCodes.Status403Forbidden);
                    }
                    else if (innerException is SecurityTokenInvalidSignatureException)
                    {
                        throw new AuthenticationException(ExceptionCodes.IDENTITY_AUTHORIZATION_INVALID_SIGNATURE, "Signature of the token is not valid", innerException, StatusCodes.Status403Forbidden);
                    }
                    else if (innerException is SecurityTokenInvalidSigningKeyException)
                    {
                        throw new AuthenticationException(ExceptionCodes.IDENTITY_AUTHORIZATION_INVALID_SIGNING, "Signing of the token is not valid", innerException, StatusCodes.Status403Forbidden);
                    }
                    else
                    {
                        throw new ExceptionBase(ExceptionCodes.UNHANDLED_EXCEPTION, "Unhandled exception", innerException, StatusCodes.Status500InternalServerError);
                    }
                }
            }
            else
            {
                throw new AuthenticationException(ExceptionCodes.IDENTITY_INVALID_AUTHORIZATION_TOKEN, "Invalid authorization token", null, StatusCodes.Status401Unauthorized);
            }
        }

        public JwtSecurityToken Get()
        {
            return _securityToken;
        }
    }
}
