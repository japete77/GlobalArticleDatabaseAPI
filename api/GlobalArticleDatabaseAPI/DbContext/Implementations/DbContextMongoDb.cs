using Config.Interfaces;
using GlobalArticleDatabase.DbContext.MongoDB.Interfaces;
using GlobalArticleDatabaseAPI.DbContext.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalArticleDatabase.DbContext.MongoDB.Implementations
{
    public class DbContextMongoDb : IDbContextMongoDb
    {
        private readonly ISettings _settings;
        private IMongoDatabase _database;
        private MongoClient _mongoClient;

        public DbContextMongoDb(ISettings settings)
        {

            _settings = settings;

            var clientSettings = MongoClientSettings.FromUrl(new MongoUrl(_settings.Server));
            _mongoClient = new MongoClient(clientSettings);

            _database = _mongoClient.GetDatabase(_settings.Database);

            Map();

            SetupConventions();
        }

        public void Connect(string database, string dbServer = null)
        {
            var clientSettings = dbServer != null ?
                MongoClientSettings.FromUrl(new MongoUrl(dbServer)) :
                MongoClientSettings.FromUrl(new MongoUrl(_settings.Database));

            _mongoClient = new MongoClient(clientSettings);
            _database = _mongoClient.GetDatabase(database);
        }

        public async Task<bool> GetConnectionStatusAsync()
        {
            var pingCommand = new BsonDocumentCommand<BsonDocument>(new BsonDocument { { "connectionStatus", 1 } });
            var result = await _database.RunCommandAsync(pingCommand);

            var dict = result.ToDictionary();

            return dict.ContainsKey("ok") && dict["ok"].ToString() == "1";
        }

        public async Task DropDatabaseAsync(string name, CancellationToken cancellationToken = default)
        {
            await _mongoClient.DropDatabaseAsync(name, cancellationToken);
        }

        public async Task CreateIndices()
        {
            await CreateIndexIfNotExits<ArticleEntity>(Articles, "IDX_ARTICLES_AUTHOR", x => x.Author, false, true);
            await CreateIndexIfNotExits<ArticleEntity>(Articles, "IDX_ARTICLES_CATEGORY", x => x.Category, false, true);
            await CreateIndexIfNotExits<ArticleEntity>(Articles, "IDX_ARTICLES_TITLE", x => x.Title, false, true, true);
            await CreateIndexIfNotExits<ArticleEntity>(Articles, "IDX_ARTICLES_DATE", x => x.Date, false, true);
            await CreateIndexIfNotExits<ArticleEntity>(Articles, "IDX_ARTICLES_OWNER", x => x.Owner, false, true);
        }

        private async Task CreateIndexIfNotExits<T>(IMongoCollection<T> collection, string name, Expression<Func<T, object>> fieldDefinition, bool unique = false, bool ascending = true, bool textIndex = false)
        {
            CreateIndexModel<T> index;

            if (textIndex)
            {
                index = new CreateIndexModel<T>(
                    ascending ? Builders<T>.IndexKeys.Text(fieldDefinition) : Builders<T>.IndexKeys.Text(fieldDefinition),
                    new CreateIndexOptions { Unique = unique, Name = name }
                );
            }
            else
            {
                index = new CreateIndexModel<T>(
                    ascending ? Builders<T>.IndexKeys.Ascending(fieldDefinition) : Builders<T>.IndexKeys.Descending(fieldDefinition),
                    new CreateIndexOptions { Unique = unique, Name = name }
                );
            }

            // Index is created if not exists... otherwise it´s a NOP
            await collection.Indexes.CreateOneAsync(index);
        }

        public bool IsValidCollectionName(string collectionName)
        {
            // Collection names should begin with an underscore or a letter character, and cannot:
            //    contain the $.
            //    be an empty string(e.g. "").
            //    contain the null character.
            //    begin with the system. prefix. (Reserved for internal use.)

            // mongo db is case insensitive so we normalize to lowercase            
            var normalizedCollectionName = collectionName.ToLower();

            // validate collection name
            return (
                !string.IsNullOrEmpty(normalizedCollectionName) &&
                !normalizedCollectionName.Contains("$") &&
                !normalizedCollectionName.Contains("system") &&
                Regex.Match(normalizedCollectionName, "^[_a-z]").Success
            );
        }

        public IMongoCollection<ArticleEntity> Articles
        {
            get
            {
                return GetCollection<ArticleEntity>("articles");
            }
        }

        public IMongoCollection<UserEntity> Users
        {
            get
            {
                return GetCollection<UserEntity>("users");
            }
        }

        public IMongoCollection<AuthRenewEntity> RenewTokens
        {
            get
            {
                return GetCollection<AuthRenewEntity>("renew_tokens");
            }
        }

        public IMongoCollection<BsonDocument> GetGenericArticlesCollection()
        {
            return GetCollection<BsonDocument>("articles");
        }

        private IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }

        private void Map()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(ArticleEntity)))
            {
                BsonClassMap.RegisterClassMap<ArticleEntity>(cm =>
                {
                    cm.AutoMap();
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(TranslationEntity)))
            {
                BsonClassMap.RegisterClassMap<TranslationEntity>(cm =>
                {
                    cm.AutoMap();
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(PublicationEntity)))
            {
                BsonClassMap.RegisterClassMap<PublicationEntity>(cm =>
                {
                    cm.AutoMap();
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(UserEntity)))
            {
                BsonClassMap.RegisterClassMap<UserEntity>(cm =>
                {
                    cm.AutoMap();
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(AuthRenewEntity)))
            {
                BsonClassMap.RegisterClassMap<AuthRenewEntity>(cm =>
                {
                    cm.AutoMap();
                });
            }
        }

        private void SetupConventions()
        {
            var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);
        }

        public void Initialize()
        {
            Map();

            SetupConventions();
        }
    }
}
