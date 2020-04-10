using GlobalArticleDatabaseAPI.Repositories.Interfaces;
using GlobalArticleDatabaseAPI.Services.Authentication.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Services.Authentication.Implementations
{
    public class RenewTokenRemover : IRenewTokenRemover
    {
        IAuthRenewRepository _authRenewRepository { get; }

        public RenewTokenRemover(IAuthRenewRepository authRenewRepository)
        {
            _authRenewRepository = authRenewRepository ?? throw new ArgumentNullException(nameof(authRenewRepository));
        }

        public async Task DeleteAsync(string renewToken, CancellationToken cancellationToken = default)
        {
            await _authRenewRepository.DeleteAsync(renewToken, cancellationToken);
        }

        public async Task DeleteByUserTokenAsync(string userToken, CancellationToken cancellationToken = default)
        {
            await _authRenewRepository.DeleteByUserTokenAsync(userToken, cancellationToken);
        }
    }
}
