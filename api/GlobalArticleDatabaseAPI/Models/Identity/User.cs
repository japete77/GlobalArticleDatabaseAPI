using FluentValidation;
using System;
using System.Collections.Generic;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Object that represents a user in the database
    /// </summary>
    public class User : BaseModel<User, UserValidator>
    {
        /// <summary>
        /// Unique Id generated internally by the database
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Name that uniquely identifies the user in the application
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Normalized user name used to validate the case insensitive uniqueness
        /// </summary>
        public string NormalizedUserName { get; set; }
        /// <summary>
        /// Password transformed to prevent someone who gains unauthorized access to the database can retrieve the clean passwords
        /// </summary>
        public string PasswordHash { get; set; }
        /// <summary>
        /// User email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Normalized user email
        /// </summary>
        public string NormalizedEmail { get; set; }
        /// <summary>
        /// Email has been confirmed
        /// </summary>
        public bool EmailConfirmed { get; set; }
        /// <summary>
        /// Token to reset user password
        /// </summary>
        public string ResetPasswordToken { get; set; }
        /// <summary>
        /// Expiry date of the reset password token
        /// </summary>
        public DateTime? ResetPasswordTokenExpiryDate { get; set; }
        /// <summary>
        /// User roles
        /// </summary>
        public List<Role> Roles { get; set; }
        /// <summary>
        /// User logo in Base64 format
        /// </summary>
        public string Photo { get; set; }
    }

    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(model => model.UserName).NotEmpty();
            RuleFor(model => model.Email).NotEmpty().EmailAddress();
        }
    }
}
