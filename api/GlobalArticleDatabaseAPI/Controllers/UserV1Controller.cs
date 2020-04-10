using Alintia.Glass.Services.User.Interfaces;
using AutoMapper;
using Core.Exceptions;
using GlobalArticleDatabaseAPI.Helpers;
using GlobalArticleDatabaseAPI.Identity.Helpers;
using GlobalArticleDatabaseAPI.Models;
using GlobalArticleDatabaseAPI.Services.Authentication.Implementations;
using GlobalArticleDatabaseAPI.Services.User.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    [Produces("application/json")]
    public class UserV1Controller : ControllerBase
    {
        UserManager<User> _userManager { get; }
        IHttpContextAccessor _contextAccesor { get; }
        IUserRetriever _userRetriever { get; }
        IUserUpdater _userUpdater { get; }
        IMapper _mapper { get; }

        public UserV1Controller(UserManager<User> userManager,
            IHttpContextAccessor contextAccesor,
            IUserRetriever userRetriever,
            IUserUpdater userUpdater,
            IMapper mapper)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _contextAccesor = contextAccesor ?? throw new ArgumentNullException(nameof(contextAccesor));
            _userRetriever = userRetriever ?? throw new ArgumentNullException(nameof(userRetriever));
            _userUpdater = userUpdater ?? throw new ArgumentNullException(nameof(userUpdater));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="request">Username and password</param>
        /// <returns>Create user response</returns>
        [HttpPost]
        [Route("user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<CreateUserResponse> Create(CreateUserRequest request)
        {
            request.ValidateAndThrow();

            var result = await _userManager.CreateAsync(request.User);

            if (!result.Succeeded)
            {
                throw new AuthenticationException(ExceptionCodes.IDENTITY_ERROR_CREATING_USER, IdentityHelper.ErrorsToString(result), null, StatusCodes.Status400BadRequest);
            }

            var userCreated = await _userManager.FindByNameAsync(request.User.UserName);

            return new CreateUserResponse
            {
                User = userCreated
            };
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="request">User to update</param>
        [HttpPut]
        [Route("user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task Update(UpdateUserRequest request)
        {
            request.ValidateAndThrow();

            var user = await _userManager.FindByIdAsync(request.User.Id);

            if (user == null)
            {
                throw new AuthenticationException(ExceptionCodes.IDENTITY_USER_NOT_EXIST, "User not found", null, StatusCodes.Status404NotFound);
            }

            if (user.NormalizedUserName == Constants.DefaultAdminUser.NormalizedUsername)
            {
                // Override roles to prevent modifications on 'admin' user
                request.User.Roles = user.Roles.Select(s => _mapper.Map<Role>(s)).ToList();
            }

            await _userUpdater.UpdateBasicAsync(request.User);
        }

        /// <summary>
        /// Retrieve an existing user
        /// </summary>
        /// <param name="id">User unique identifier</param>
        [HttpGet]
        [Route("user/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<GetUserResponse> Get(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new AuthenticationException(ExceptionCodes.IDENTITY_USER_NOT_EXIST, "User not found", null, StatusCodes.Status404NotFound);
            }

            return new GetUserResponse
            {
                User = user
            };
        }

        /// <summary>
        /// Retrieve an existing user by user name
        /// </summary>
        [HttpGet]
        [Route("user/current")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<GetUserResponse> GetCurrent()
        {
            var userToken = JwtRetriever.GetUserToken(_contextAccesor.HttpContext);
            var claims = new ClaimsHelper(userToken.Claims);
            var user = await _userRetriever.GetByUserName(claims.UserName.ToUpper());

            if (user == null)
            {
                throw new AuthenticationException(ExceptionCodes.IDENTITY_USER_NOT_EXIST, "User not found", null, StatusCodes.Status404NotFound);
            }

            return new GetUserResponse
            {
                User = user
            };
        }

        /// <summary>
        /// Deletes a user
        /// </summary>
        /// <param name="id">User´s unique identificator </param>
        /// <returns>Delete user response</returns>        
        [Route("user/{id}")]
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new ExceptionBase(ExceptionCodes.IDENTITY_USER_NOT_EXIST, "User doesn´t exist", null, StatusCodes.Status400BadRequest);
            }

            if (user.NormalizedUserName == Constants.DefaultAdminUser.NormalizedUsername)
            {
                throw new ExceptionBase(ExceptionCodes.IDENTITY_ERROR_DELETING_ADMIN_USER, "Admin user can´t be deleted", null, StatusCodes.Status400BadRequest);
            }

            await _userManager.DeleteAsync(new User { Id = id });
        }

        /// <summary>
        /// Retrieve a list of users
        /// </summary>
        /// <param name="page">Page number to retrieve. Initial page number is 1</param>
        /// <param name="pageSize">Number of users per page</param>
        /// <param name="filter">Words for search query. Default value is null</param>
        /// <returns></returns>        
        [Route("users")]
        [HttpGet]
        [ProducesResponseType(typeof(SearchUsersResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<SearchUsersResponse> SearchUsers([Required]int page, [Required]int pageSize, string filter = null)
        {
            if (page == 0 || pageSize == 0)
            {
                throw new InvalidArgumentException(ExceptionCodes.INTERNAL_INVALID_API_ARGUMENTS, "Page and PageSize are mandatory", null, StatusCodes.Status400BadRequest);
            }

            var total = await _userRetriever.CountAsync(filter);

            var result = await _userRetriever.GetAllAsync(page, pageSize, filter);

            return new SearchUsersResponse
            {
                CurrentPage = page,
                Total = total,
                Users = result.ToList(),
                Next = page * pageSize < total ? $"{Request.Path}?page={page + 1}&pageSize={pageSize}" : null
            };
        }
    }
}