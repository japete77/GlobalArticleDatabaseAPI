using GlobalArticleDatabaseAPI.Models;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Services.Authentication.Interfaces
{
    public interface IRenewTokenRetriever
    {
        Task<AuthRenew> GetAsync(string renewToken, CancellationToken cancellationToken = default);
    }
}
