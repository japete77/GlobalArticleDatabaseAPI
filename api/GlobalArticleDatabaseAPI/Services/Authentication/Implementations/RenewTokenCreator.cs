using GlobalArticleDatabaseAPI.Repositories.Interfaces;
using GlobalArticleDatabaseAPI.Services.Authentication.Interfaces;
using GlobalArticleDatabaseAPI.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Services.Authentication.Implementations
{
    public class RenewTokenCreator : IRenewTokenCreator
    {
        IAuthRenewRepository _authRenewRepository { get; }

        public RenewTokenCreator(IAuthRenewRepository authRenewRepository)
        {
            _authRenewRepository = authRenewRepository ?? throw new ArgumentNullException(nameof(authRenewRepository));
        }

        public async Task AddAsync(AuthRenew authRenew, CancellationToken cancellationToken = default)
        {
            await _authRenewRepository.InsertAsync(authRenew, cancellationToken);
        }
    }
}
