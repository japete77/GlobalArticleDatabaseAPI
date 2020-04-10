using GlobalArticleDatabaseAPI.Models;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Services.Authentication.Interfaces
{
    public interface IRenewTokenCreator
    {
        Task AddAsync(AuthRenew authRenew, CancellationToken cancellationToken = default);
    }
}
