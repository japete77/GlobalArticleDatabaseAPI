using FluentValidation;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Update user request
    /// </summary>
    public class UpdateUserRequest : BaseModel<UpdateUserRequest, UpdateUserRequestValidator>
    {
        /// <summary>
        /// User
        /// </summary>
        public User User { get; set; }
    }

    /// <summary>
    /// Update user request validator
    /// </summary>
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public UpdateUserRequestValidator()
        {
            RuleFor(model => model.User).NotEmpty();
            RuleFor(model => model.User).SetValidator(new UserValidator());
        }
    }
}
