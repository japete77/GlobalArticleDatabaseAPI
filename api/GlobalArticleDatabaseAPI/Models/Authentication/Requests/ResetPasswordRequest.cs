using FluentValidation;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Reset password request
    /// </summary>
    public class ResetPasswordRequest : BaseModel<ResetPasswordRequest, ResetPasswordRequestValidator>
    {
        /// <summary>
        /// Valid reset password token
        /// </summary>
        public string ResetPasswordToken { get; set; }
        /// <summary>
        /// User email address
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// New user password
        /// </summary>
        public string NewPassword { get; set; }
        /// <summary>
        /// User tenant
        /// </summary>
        public string Tenant { get; set; }
    }

    /// <summary>
    /// Reset password request validator
    /// </summary>
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ResetPasswordRequestValidator()
        {
            RuleFor(model => model.ResetPasswordToken).NotEmpty();
            RuleFor(model => model.Email).NotEmpty().EmailAddress();
            RuleFor(model => model.NewPassword).NotEmpty();
            RuleFor(model => model.Tenant).NotEmpty();
        }
    }
}
