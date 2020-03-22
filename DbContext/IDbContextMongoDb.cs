using GlobalArticleDatabaseAPI.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace DataAccess.DbContext.MongoDB.Interfaces
{
    public interface IDbContextMongoDb
    {
        Task<bool> GetConnectionStatusAsync();
        void Connect(string database, string dbServer = null);
        IMongoCollection<Article> Articles { get; }
        IMongoCollection<Translation> Translations { get; }
        IMongoCollection<BsonDocument> GetGenericElementCollection();
        bool IsValidCollectionName(string collectionName);
        Task DropDatabaseAsync(string name, CancellationToken cancellationToken = default);
        Task CreateIndices();
        void Initialize();
    }
}
