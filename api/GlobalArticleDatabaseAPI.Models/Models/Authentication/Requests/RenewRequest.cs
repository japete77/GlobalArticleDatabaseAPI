using FluentValidation;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Renew request
    /// </summary>
    public class RenewRequest : BaseModel<RenewRequest, RenewRequestValidator>
    {
        /// <summary>
        /// Renew token. It is returned after a successful login and required to renew the session.
        /// </summary>
        public string RenewToken { get; set; }
    }

    /// <summary>
    /// Renew request validator
    /// </summary>
    public class RenewRequestValidator : AbstractValidator<RenewRequest>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RenewRequestValidator()
        {
            RuleFor(model => model.RenewToken).NotEmpty();
        }
    }
}
