using FluentValidation;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Role
    /// </summary>
    public class Role : BaseModel<Role, RoleValidator>
    {
        /// <summary>
        /// Role unique identifier 
        /// </summary>
        public string Name { get; set; }
    }

    public class RoleValidator : AbstractValidator<Role>
    {
        public RoleValidator()
        {
            RuleFor(model => model.Name).NotEmpty();
        }
    }

}
