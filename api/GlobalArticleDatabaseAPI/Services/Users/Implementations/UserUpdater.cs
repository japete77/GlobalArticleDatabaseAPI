using Alintia.Glass.Services.User.Interfaces;
using GlobalArticleDatabaseAPI.Repositories.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using BModels = GlobalArticleDatabaseAPI.Models;

namespace Alintia.Glass.Api.Services.User.Implementations
{
    public class UserUpdater : IUserUpdater
    {
        IUserRepository _userRepository { get; }

        public UserUpdater(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task UpdateAsync(BModels.User user, CancellationToken cancellationToken = default)
        {
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        public async Task UpdateBasicAsync(BModels.User user, CancellationToken cancellationToken = default)
        {
            await _userRepository.UpdateBasicAsync(user, cancellationToken);
        }
    }
}
