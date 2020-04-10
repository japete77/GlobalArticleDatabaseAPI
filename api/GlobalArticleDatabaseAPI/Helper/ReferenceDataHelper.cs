using Config.Interfaces;
using GlobalArticleDatabaseAPI.Models;
using GlobalArticleDatabaseAPI.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using static GlobalArticleDatabaseAPI.Constants;

namespace GlobalArticleDatabaseAPI.Helper
{
    public class ReferenceDataHelper
    {
        public async Task CreateDefaultAdminUser()
        {
            var userRepository = Startup.GetService<IUserRepository>();

            var settings = Startup.GetService<ISettings>();

            var numUsers = await userRepository.CountAsync(null);

            if (numUsers == 0)
            {
                // we have to create a default admin user
                await userRepository.InsertAsync(new User()
                {
                    UserName = DefaultAdminUser.Username,
                    NormalizedUserName = DefaultAdminUser.NormalizedUsername,
                    Email = settings.DefaultEmail,
                    NormalizedEmail = settings.DefaultEmail?.ToUpper(),
                    PasswordHash = DefaultAdminUser.PasswordHash,
                    Roles = new List<Role>
                    {
                        new Role
                        {
                            Name = Roles.Admin
                        }
                    }
                });
            }
        }
    }
}
