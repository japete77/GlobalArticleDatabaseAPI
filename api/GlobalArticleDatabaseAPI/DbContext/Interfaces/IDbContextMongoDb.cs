using GlobalArticleDatabaseAPI.DbContext.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.DbContext.MongoDB.Interfaces
{
    public interface IDbContextMongoDb
    {
        Task<bool> GetConnectionStatusAsync();
        void Connect(string database, string dbServer = null);
        IMongoCollection<ArticleEntity> Articles { get; }
        IMongoCollection<UserEntity> Users { get; }
        IMongoCollection<AuthRenewEntity> RenewTokens { get; }
        IMongoCollection<BsonDocument> GetGenericArticlesCollection();
        bool IsValidCollectionName(string collectionName);
        Task DropDatabaseAsync(string name, CancellationToken cancellationToken = default);
        Task CreateIndices();
        void Initialize();
    }
}
