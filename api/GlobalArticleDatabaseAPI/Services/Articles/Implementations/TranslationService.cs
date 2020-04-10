using AutoMapper;
using Config.Interfaces;
using Core.Exceptions;
using GlobalArticleDatabaseAPI.DbContext.MongoDB.Interfaces;
using GlobalArticleDatabaseAPI.Helpers;
using GlobalArticleDatabaseAPI.DbContext.Models;
using GlobalArticleDatabaseAPI.Models;
using GlobalArticleDatabaseAPI.Services.Articles.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
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

        public async Task Create(CreateTranslationRequest request)
        {
            var exists = await Exists(request.ArticleId, request.Translation.Language);

            if (exists) throw new DuplicateKeyException(ExceptionCodes.TRANSLATION_ALREADY_EXISTS, $"Translation with language '{request.Translation.Language}' already exists");

            var matchConditions = new FilterDefinition<BsonDocument>[]
            {
                new BsonDocument(
                    "_id",
                    new ObjectId(request.ArticleId)
                ),
            };

            request.Translation.HasText = !string.IsNullOrEmpty(request.Text);

            await _dbContext.GetGenericArticlesCollection().UpdateOneAsync(
                Builders<BsonDocument>.Filter.And(matchConditions),
                Builders<BsonDocument>.Update.Push(
                    BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations),
                    _mapper.Map<TranslationEntity>(request.Translation)
                ),
                new UpdateOptions { IsUpsert = false }
            );

            if (request.Translation.HasText)
            {
                var s3Client = _s3Client.GetClient();

                var result = await s3Client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
                {
                    BucketName = _settings.AWSBucket,
                    Key = GetTranslationTextFilename(request.ArticleId, request.Translation),
                    ContentBody = request.Text,
                    ContentType = "text/html; charset=utf-8"
                });

                if (result.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    // Rollback
                    await Delete(request.ArticleId, request.Translation.Language);

                    throw new InternalException(ExceptionCodes.TRANSLATION_ERROR_UPLOADING_TEXT, $"Error uploading translation with language '{request.Translation.Language}'");
                }
            }
        }

        public async Task Update(UpdateTranslationRequest request)
        {
            var exists = await Exists(request.ArticleId, request.Translation.Language);

            if (!exists) throw new DuplicateKeyException(ExceptionCodes.TRANSLATION_NOT_FOUND, $"Translation with language '{request.Translation.Language}' not found", null, (int)System.Net.HttpStatusCode.NotFound);

            var matchConditions = new FilterDefinition<BsonDocument>[]
            {
                new BsonDocument(
                    "_id",
                    new ObjectId(request.ArticleId)
                ),
                new BsonDocument(
                    BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations),
                    new BsonDocument("$elemMatch", new BsonDocument(BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Language), request.Translation.Language))
                ),
            };

            var updateDefinition = new BsonDocument(
                "$set", new BsonDocument
                {
                    { $"{BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations)}.$.{BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Title)}", NormalizeNull(request.Translation.Title) },
                    { $"{BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations)}.$.{BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Subtitle)}", NormalizeNull(request.Translation.Subtitle) },
                    { $"{BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations)}.$.{BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Summary)}", NormalizeNull(request.Translation.Summary) },
                    { $"{BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations)}.$.{BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Date)}", request.Translation.Date },
                    { $"{BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations)}.$.{BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.ReviewedBy)}", NormalizeNull(request.Translation.ReviewedBy) },
                    { $"{BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations)}.$.{BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Status)}", NormalizeNull(request.Translation.Status) },
                    { $"{BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations)}.$.{BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.TranslatedBy)}", NormalizeNull(request.Translation.TranslatedBy) },
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

            var s3Client = _s3Client.GetClient();

            await s3Client.DeleteAsync(
                _settings.AWSBucket,
                GetTranslationTextFilename(articleId, new Translation { Language = language }),
                null
            );
        }

        public async Task UpdateText(UpdateTranslationTextRequest request)
        {
            // Update HasText flag
            var matchConditions = new FilterDefinition<BsonDocument>[]
            {
                new BsonDocument(
                    "_id",
                    new ObjectId(request.ArticleId)
                ),
                new BsonDocument(
                    BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations),
                    new BsonDocument("$elemMatch", new BsonDocument(BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Language), request.Language))
                ),
            };

            var updateDefinition = new BsonDocument(
                "$set", new BsonDocument
                {
                    { $"{BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations)}.$.{BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.HasText)}", true }
                }
            );

            await _dbContext.GetGenericArticlesCollection().UpdateOneAsync(
                Builders<BsonDocument>.Filter.And(matchConditions),
                updateDefinition
            );

            // Update file
            var s3Client = _s3Client.GetClient();

            var result = await s3Client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
            {
                BucketName = _settings.AWSBucket,
                Key = GetTranslationTextFilename(request.ArticleId, new Translation { Language = request.Language }),
                ContentBody = request.Text
            });

            if (result.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalException(ExceptionCodes.TRANSLATION_ERROR_UPLOADING_TEXT, $"Error uploading translation with language '{request.Language}'");
            }
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

        private string GetTranslationTextFilename(string articleId, Translation translation)
        {
            return $"{articleId}-{translation.Language}.txt";
        }
    }
}
