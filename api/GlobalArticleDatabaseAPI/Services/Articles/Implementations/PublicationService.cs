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
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Services.Articles.Implementations
{
    public class PublicationService : IPublicationService
    {
        IDbContextMongoDb _dbContext { get; }
        IMapper _mapper { get; }

        public PublicationService(IDbContextMongoDb dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task Create(CreatePublicationRequest request)
        {
            var exists = await Exists(request.ArticleId, request.Language, request.Publication.Publisher);

            if (exists) throw new DuplicateKeyException(ExceptionCodes.PUBLICATION_ALREADY_EXISTS, $"Publication with language '{request.Language}' and published by '{request.Publication.Publisher}' already exists");

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

            await _dbContext.GetGenericArticlesCollection().UpdateOneAsync(
                Builders<BsonDocument>.Filter.And(matchConditions),
                Builders<BsonDocument>.Update.Push(
                    $"{BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations)}.$.{BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Publications)}",
                    _mapper.Map<PublicationEntity>(request.Publication)
                ),
                new UpdateOptions { IsUpsert = false }
            );
        }

        public async Task Delete(string articleId, string language, string publisher)
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

            var deleteDefinition = new BsonDocument(
                "$pull", new BsonDocument(
                    $"{BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations)}.$.{BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Publications)}",
                    new BsonDocument(
                        BsonPropertyHelper.GetPropertyName<PublicationEntity>(f => f.Publisher),
                        publisher
                    )
                )
            );

            await _dbContext.GetGenericArticlesCollection().UpdateOneAsync(
                Builders<BsonDocument>.Filter.And(matchConditions),
                deleteDefinition
            );
        }

        private async Task<bool> Exists(string articleId, string language, string publisher)
        {
            var matchConditions = new FilterDefinition<BsonDocument>[]
            {
                new BsonDocument(
                    "_id",
                    new ObjectId(articleId)
                ),
                new BsonDocument(
                    BsonPropertyHelper.GetPropertyName<ArticleEntity>(f => f.Translations),
                    new BsonDocument
                    {
                        {
                            "$elemMatch",  new BsonDocument {
                                { BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Language), language },
                                { 
                                    BsonPropertyHelper.GetPropertyName<TranslationEntity>(f => f.Publications), new BsonDocument { 
                                        { 
                                            "$elemMatch", new BsonDocument {
                                                { BsonPropertyHelper.GetPropertyName<PublicationEntity>(f => f.Publisher), publisher }
                                            }
                                        } 
                                    } 
                                }
                            }
                        }
                    }
                ),
            };

            var result = await _dbContext.GetGenericArticlesCollection().FindAsync(
                Builders<BsonDocument>.Filter.And(matchConditions)
            );

            var articles = await result.ToListAsync();

            return articles.Count > 0;
        }
    }
}
