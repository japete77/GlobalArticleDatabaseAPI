using FluentValidation;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Forgot password request
    /// </summary>
    public class ForgotPasswordRequest : BaseModel<ForgotPasswordRequest, ForgotPasswordRequestValidator>
    {
        /// <summary>
        /// User email address
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// User tenant
        /// </summary>
        public string Tenant { get; set; }
    }

    /// <summary>
    /// Forgot password request validator
    /// </summary>
    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ForgotPasswordRequestValidator()
        {
            RuleFor(model => model.Email).NotEmpty().EmailAddress();
            RuleFor(model => model.Tenant).NotEmpty();
        }
    }
}
