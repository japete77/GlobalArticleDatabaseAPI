using GlobalArticleDatabaseAPI.Models;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Repositories.Interfaces
{
    public interface IAuthRenewRepository
    {
        AuthRenew GetByUserToken(string userToken);
        Task<AuthRenew> GetAsync(string renewToken, CancellationToken cancellationToken = default);
        Task InsertAsync(AuthRenew authRenew, CancellationToken cancellationToken = default);
        Task DeleteAsync(string renewToken, CancellationToken cancellationToken = default);
        Task DeleteByUserTokenAsync(string userToken, CancellationToken cancellationToken = default);
    }
}
