using FluentValidation;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Login request
    /// </summary>
    public class LoginRequest : BaseModel<LoginRequest, LoginRequestValidator>
    {
        /// <summary>
        /// Name that uniquely identifies the user in the application
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Secret word or phrase that must be used to gain admission to the application.
        /// Passwords must be at least 6 characters. 
        /// Passwords must have at least one lowercase ('a'-'z') 
        /// Passwords must have at least one uppercase ('A'-'Z')
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// Login request validator
    /// </summary>
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LoginRequestValidator()
        {
            RuleFor(model => model.User).NotEmpty();
            RuleFor(model => model.Password).NotEmpty();
        }
    }
}
