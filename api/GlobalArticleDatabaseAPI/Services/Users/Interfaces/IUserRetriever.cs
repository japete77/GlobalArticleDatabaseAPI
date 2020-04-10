using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BModels = GlobalArticleDatabaseAPI.Models;

namespace GlobalArticleDatabaseAPI.Services.User.Interfaces
{
    public interface IUserRetriever
    {
        Task<long> CountAsync(string filter, CancellationToken cancellationToken = default);
        Task<List<BModels.User>> GetAllAsync(int page, int pageSize, string filter, CancellationToken cancellationToken = default);
        Task<BModels.User> GetByUserName(string userName, CancellationToken cancellationToken = default);
        Task<BModels.User> GetById(string id, CancellationToken cancellationToken = default);
    }
}
