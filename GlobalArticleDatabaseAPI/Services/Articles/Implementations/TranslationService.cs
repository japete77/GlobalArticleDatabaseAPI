using AutoMapper;
using Config.Interfaces;
using Core.Exceptions;
using DataAccess.DbContext.MongoDB.Interfaces;
using GlobalArticleDatabaseAPI.DbContext.Models;
using GlobalArticleDatabaseAPI.Helper;
using GlobalArticleDatabaseAPI.Models;
using GlobalArticleDatabaseAPI.Services.Articles.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Services.Articles.Implementations
{
    public class TranslationService : ITranslationService
    {
        IDbContextMongoDb _dbContext { get; }
        ISettings _settings { get; }
        IMapper _mapper { get; }
        IS3Client _s3Client { get; }

        public TranslationService(IDbContextMongoDb dbContext, ISettings settings, IMapper mapper, IS3Client s3Client)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        }

        public async Task Create(string articleId, Translation translation)
        {
            var exists = await Exists(articleId, translation.Language);

            if (exists) throw new DuplicateKeyException(ExceptionCodes.TRANSLATION_ALREADY_EXISTS, $"Translation with languag '{translation.Language}' already exists");

            var matchConditions = new FilterDefinition<BsonDocument>[]
            {
                new BsonDocument(
                    "_id",
                    new ObjectId(articleId)
                ),
            };

            await _dbContext.GetGenericArticlesCollection().UpdateOneAsync(
                Builders<BsonDocument>.Filter.And(matchConditions),
                Builders<BsonDocument>.Update.Push(
                    BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations),
                    translation
                ),
                new UpdateOptions { IsUpsert = false }
            );
        }

        public async Task Update(string articleId, Translation translation)
        {
            var exists = await Exists(articleId, translation.Language);

            if (!exists) throw new DuplicateKeyException(ExceptionCodes.TRANSLATION_NOT_FOUND, $"Translation with languag '{translation.Language}' not found");

            var matchConditions = new FilterDefinition<BsonDocument>[]
            {
                new BsonDocument(
                    "_id",
                    new ObjectId(articleId)
                ),
                new BsonDocument(
                    BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations),
                    new BsonDocument("$elemMatch", new BsonDocument(BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Language), translation.Language))
                ),
            };

            var updateDefinition = new BsonDocument(
                "$set", new BsonDocument
                {
                    { $"{BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations)}.$.{BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Title)}", NormalizeNull(translation.Title) },
                    { $"{BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations)}.$.{BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Subtitle)}", NormalizeNull(translation.Subtitle) },
                    { $"{BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations)}.$.{BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Summary)}", NormalizeNull(translation.Summary) },
                    { $"{BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations)}.$.{BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Date)}", translation.Date },
                    { $"{BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations)}.$.{BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.ReviewedBy)}", NormalizeNull(translation.ReviewedBy) },
                    { $"{BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations)}.$.{BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Status)}", NormalizeNull(translation.Status) },
                    { $"{BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations)}.$.{BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.TranslatedBy)}", NormalizeNull(translation.TranslatedBy) },
                }
            );

            await _dbContext.GetGenericArticlesCollection().UpdateOneAsync(
                Builders<BsonDocument>.Filter.And(matchConditions),
                updateDefinition
            );
        }

        public async Task Delete(string articleId, string language)
        {
            var matchConditions = new FilterDefinition<BsonDocument>[]
            {
                new BsonDocument(
                    "_id",
                    new ObjectId(articleId)
                ),
            };

            var deleteDefinition = new BsonDocument(
                "$pull", new BsonDocument(
                    BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations),
                    new BsonDocument(
                        BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Language),
                        language
                    )
                )
            ); 

            await _dbContext.GetGenericArticlesCollection().UpdateOneAsync(
                Builders<BsonDocument>.Filter.And(matchConditions),
                deleteDefinition
            );
        }

        private async Task<bool> Exists(string articleId, string language)
        {
            var matchConditions = new FilterDefinition<BsonDocument>[]
            {
                new BsonDocument(
                    "_id",
                    new ObjectId(articleId)
                ),
                new BsonDocument(
                    BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations),
                    new BsonDocument("$elemMatch", new BsonDocument(BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Language), language))
                ),
            };

            var result = await _dbContext.GetGenericArticlesCollection().FindAsync(
                Builders<BsonDocument>.Filter.And(matchConditions)
            );

            var articles = await result.ToListAsync();

            return articles.Count > 0;
        }

        private string NormalizeNull(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value;
        }
    }
}
