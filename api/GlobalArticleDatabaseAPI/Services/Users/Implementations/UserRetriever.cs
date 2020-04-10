using GlobalArticleDatabaseAPI.Repositories.Interfaces;
using GlobalArticleDatabaseAPI.Services.User.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BModels = GlobalArticleDatabaseAPI.Models;

namespace GlobalArticleDatabaseAPI.Services.User.Implementations
{
    public class UserRetriever : IUserRetriever
    {
        IUserRepository _userRepository { get; }

        public UserRetriever(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<long> CountAsync(string filter, CancellationToken cancellationToken = default)
        {
            return await _userRepository.CountAsync(filter, cancellationToken);
        }

        public async Task<List<BModels.User>> GetAllAsync(int page, int pageSize, string filter, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetAllAsync(page, pageSize, filter, cancellationToken);
        }

        public async Task<BModels.User> GetByUserName(string userName, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByUserNameAsync(userName, cancellationToken);
        }

        public async Task<BModels.User> GetById(string id, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByIdAsync(id, cancellationToken);
        }
    }
}
