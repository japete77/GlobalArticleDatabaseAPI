using System.Threading;
using System.Threading.Tasks;
using BModels = GlobalArticleDatabaseAPI.Models;

namespace Alintia.Glass.Services.User.Interfaces
{
    public interface IUserUpdater
    {
        Task UpdateAsync(BModels.User user, CancellationToken cancellationToken = default);
        Task UpdateBasicAsync(BModels.User user, CancellationToken cancellationToken = default);
    }
}
