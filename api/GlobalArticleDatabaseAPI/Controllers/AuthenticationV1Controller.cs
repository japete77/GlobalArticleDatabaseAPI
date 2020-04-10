using Core.Exceptions;
using GlobalArticleDatabaseAPI.Helpers;
using GlobalArticleDatabaseAPI.Identity.Helpers;
using GlobalArticleDatabaseAPI.Services.Authentication.Interfaces;
using GlobalArticleDatabaseAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Alintia.Glass.Api.Controllers
{
    [Route("/api/v1/auth/")]
    [ApiController]
    [Produces("application/json")]
    public class AuthenticationV1Controller : ControllerBase
    {
        UserManager<User> _userManager { get; }
        IJwtGenerator _jwtGenereator { get; }
        IRenewTokenCreator _renewTokenCreator { get; }
        IRenewTokenRemover _renewTokenRemover { get; }
        IRenewTokenRetriever _renewTokenRetriever { get; }
        IJwtRetriever _jwtRetriever { get; }

        public AuthenticationV1Controller(UserManager<User> userManager,
            IJwtGenerator jwtGenereator,
            IRenewTokenCreator renewTokenCreator,
            IRenewTokenRemover renewTokenRemover,
            IRenewTokenRetriever renewTokenRetriever,
            IJwtRetriever jwtRetriever)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _jwtGenereator = jwtGenereator ?? throw new ArgumentNullException(nameof(jwtGenereator));
            _renewTokenCreator = renewTokenCreator ?? throw new ArgumentNullException(nameof(renewTokenCreator));
            _renewTokenRemover = renewTokenRemover ?? throw new ArgumentNullException(nameof(renewTokenRemover));
            _renewTokenRetriever = renewTokenRetriever ?? throw new ArgumentNullException(nameof(renewTokenRetriever));
            _jwtRetriever = jwtRetriever ?? throw new ArgumentNullException(nameof(jwtRetriever));
        }

        /// <summary>
        /// Log in to the application
        /// </summary>
        /// <param name="request">User name and password credential to access to the application</param>                
        [Route("login")]
        [HttpPost]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<LoginResponse> Login(LoginRequest request)
        {
            request.ValidateAndThrow();

            // check user name
            var user = await _userManager.FindByNameAsync(request.User);

            var validCredentials = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!validCredentials)
            {
                throw new AuthenticationException(ExceptionCodes.IDENTITY_INVALID_USER_PASSWORD, "Invalid user name or password", null, StatusCodes.Status401Unauthorized);
            }

            // we add user claims
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimsHelper.ID, Guid.NewGuid().ToString()),
                new Claim(ClaimsHelper.USERNAME_KEY, request.User),
            };
            claims.AddRange(user.Roles.Select(s => new Claim(ClaimsHelper.ROLE_KEY, s.Name)).ToList());

            List<Claim> claimsRenew = new List<Claim>
            {
                new Claim(ClaimsHelper.ID, Guid.NewGuid().ToString()),
            };

            // Create and register renew token
            string userToken = _jwtGenereator.Create(claims);
            string renewToken = _jwtGenereator.Create(claimsRenew);

            await _renewTokenCreator.AddAsync(new AuthRenew
            {
                UserToken = userToken,
                RenewToken = renewToken,
                ExpiteAt = DateTime.UtcNow.AddMinutes(GlobalArticleDatabaseAPI.Constants.App.RenewTokenPasswordExpirationInMinutes)
            });

            return new LoginResponse()
            {
                Token = userToken,
                RenewToken = renewToken
            };
        }

        /// <summary>
        /// Start the forgot password workflow
        /// </summary>
        /// <param name="request">Forgot password request model</param>
        [Route("forgot-password")]
        [HttpPost]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task ForgotPassword(ForgotPasswordRequest request)
        {
            request.ValidateAndThrow();

            var user = await _userManager.FindByEmailAsync(request.Email);

            // if user doesn´t exist we just do a silent fail to prevent hackers to get info 
            // about users in the database based on forgot password responses
            if (user != null)
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                //await _workflowRepository.CreateNewWorkflow(new WorkflowInstance
                //{
                //    WorkflowDefinitionId = WorkflowDefinitions.SendEmailWorkflowId,
                //    CreateTime = DateTime.UtcNow,
                //    Status = WorkflowStatus.Runnable,
                //    Data = new SendEmailModel
                //    {
                //        To = user.Email,
                //        Subject = string.IsNullOrEmpty(user.PasswordHash) ? "Welcome to Alintia!" : "Password reset on Alintia",
                //        Content = string.IsNullOrEmpty(user.PasswordHash) ?
                //                        _emailTemplateRetriever.SetInitialPasswordTemplate(request.Tenant, user.UserName, user.Email, resetToken) :
                //                        _emailTemplateRetriever.GetResetPasswordTemplate(request.Tenant, user.UserName, user.Email, resetToken)
                //    },
                //    Reference = GetUserTenant()
                //});
            }
        }

        /// <summary>
        /// Reset user password using a token
        /// </summary>
        /// <param name="request">Reset password request model</param>
        [Route("reset-password")]
        [HttpPost]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task ResetPassword(ResetPasswordRequest request)
        {
            request.ValidateAndThrow();

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user != null)
            {
                // Check if the new password is valid
                var validator = new PasswordValidator<User>();
                var result = await validator.ValidateAsync(_userManager, user, request.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = IdentityHelper.ErrorsToString(result);
                    throw new AuthenticationException(ExceptionCodes.IDENTITY_ERROR_RESET_PASSWORD, errors, null, StatusCodes.Status400BadRequest);
                }

                result = await _userManager.ResetPasswordAsync(user, request.ResetPasswordToken, request.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = IdentityHelper.ErrorsToString(result);
                    throw new AuthenticationException(ExceptionCodes.IDENTITY_ERROR_RESET_PASSWORD, errors, null, StatusCodes.Status400BadRequest);
                }
            }
            else
            {
                throw new AuthenticationException(ExceptionCodes.IDENTITY_ERROR_RESET_PASSWORD, "Invalid request", null, StatusCodes.Status400BadRequest);
            }
        }

        /// <summary>
        /// Renew an authentication token
        /// </summary>
        /// <param name="request">Renew password request model</param>
        [Route("renew-token")]
        [HttpPost]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<RenewResponse> RenewToken(RenewRequest request)
        {
            request.ValidateAndThrow();

            var userToken = _jwtRetriever.Get();

            var authRenew = await _renewTokenRetriever.GetAsync(request.RenewToken);

            // Check if renew token exists and matches with user token
            if (authRenew == null)
            {
                throw new AuthenticationException(ExceptionCodes.IDENTITY_ERROR_RENEWING_TOKEN, "Sesion has expired. Please login again.", new Exception("Invalid renew token"), StatusCodes.Status400BadRequest);
            }
            else if (authRenew.UserToken != userToken.RawData)
            {
                throw new AuthenticationException(ExceptionCodes.IDENTITY_ERROR_RENEWING_TOKEN, "Sesion has expired. Please login again.", new Exception("User token doesn´t match with renew token"), StatusCodes.Status400BadRequest);
            }

            // Generate a new auth & renew token
            var newUserToken = _jwtGenereator.Create(userToken.Claims);
            var newRenewToken = _jwtGenereator.Create(null);

            // Remove renew token from db
            await _renewTokenRemover.DeleteAsync(request.RenewToken);

            // Register the new renew token
            await _renewTokenCreator.AddAsync(new AuthRenew
            {
                UserToken = newUserToken,
                RenewToken = newRenewToken,
                ExpiteAt = DateTime.UtcNow.AddMinutes(GlobalArticleDatabaseAPI.Constants.App.RenewTokenPasswordExpirationInMinutes)
            });

            return new RenewResponse
            {
                Token = newUserToken,
                RenewalToken = newRenewToken
            };
        }

        /// <summary>
        /// Logout from the application
        /// </summary>
        [Route("logout")]
        [HttpPost]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task Logout()
        {
            var userToken = _jwtRetriever.Get();

            try
            {
                // Remove renew token from db in silent mode
                await _renewTokenRemover.DeleteByUserTokenAsync(userToken.RawData);
            }
            catch { }
        }
    }
}
