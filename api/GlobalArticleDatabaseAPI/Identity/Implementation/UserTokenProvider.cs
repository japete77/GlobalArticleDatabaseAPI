using GlobalArticleDatabaseAPI.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace GlobalArticleDatabase.Api.Identity.Implementation
{
    public class UserTokenProvider : IUserTwoFactorTokenProvider<User>
    {
        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<User> manager, User user)
        {
            return Task.FromResult(true);
        }

        public async Task<string> GenerateAsync(string purpose, UserManager<User> manager, User user)
        {
            var token = Guid.NewGuid().ToString();

            // save in the database
            user.ResetPasswordToken = token;
            user.ResetPasswordTokenExpiryDate = DateTime.UtcNow.AddMinutes(Constants.App.ResetPasswordTokenExpirationInMinutes);
            await manager.UpdateAsync(user);

            return token;
        }

        public async Task<bool> ValidateAsync(string purpose, string token, UserManager<User> manager, User user)
        {
            // validate token
            if (user.ResetPasswordToken != token ||
                user.ResetPasswordTokenExpiryDate <= DateTime.UtcNow)
            {
                return false;
            }

            // clean up token
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiryDate = null;
            await manager.UpdateAsync(user);

            return true;
        }
    }
}
