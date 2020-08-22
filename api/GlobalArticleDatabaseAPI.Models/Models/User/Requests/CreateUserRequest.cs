using FluentValidation;
using GlobalArticleDatabaseAPI.Models;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Request object for user creation
    /// </summary>
    public class CreateUserRequest : BaseModel<CreateUserRequest, CreateUserRequestValidator>
    {
        /// <summary>
        /// User model
        /// </summary>
        public User User { get; set; }
    }

    /// <summary>
    /// Create user request validator
    /// </summary>
    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CreateUserRequestValidator()
        {
            RuleFor(model => model.User).NotEmpty();
            RuleFor(model => model.User).SetValidator(new UserValidator());
        }
    }
}
