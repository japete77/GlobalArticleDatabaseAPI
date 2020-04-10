using GlobalArticleDatabaseAPI.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<User> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
        Task<User> GetByUserNameAsync(string normalizedUsername, CancellationToken cancellationToken = default);
        Task<IList<User>> GetByRoleAsync(string roleName, CancellationToken cancellationToken = default);
        Task<List<User>> GetAllAsync(int page, int pageSize, string filter, CancellationToken cancellationToken = default);
        Task<long> CountAsync(string filter, CancellationToken cancellationToken = default);
        long Count(string filter, CancellationToken cancellationToken = default);
        Task InsertAsync(User user, CancellationToken cancellationToken = default);
        void Insert(User user, CancellationToken cancellationToken = default);
        Task UpdateAsync(User user, CancellationToken cancellationToken = default);
       Task UpdateBasicAsync(User user, CancellationToken cancellationToken = default);
        Task DeleteAsync(User user, CancellationToken cancellationToken = default);
    }
}
