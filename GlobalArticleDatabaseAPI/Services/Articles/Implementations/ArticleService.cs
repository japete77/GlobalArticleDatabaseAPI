using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using AutoMapper;
using Config.Interfaces;
using Core.Exceptions;
using DataAccess.DbContext.MongoDB.Interfaces;
using GlobalArticleDatabase.Services.Articles.Interfaces;
using GlobalArticleDatabaseAPI.DbContext.Models;
using GlobalArticleDatabaseAPI.Models;
using GlobalArticleDatabaseAPI.Services.Articles.Interfaces;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalArticleDatabase.Services.Articles.Implementations
{
    public class ArticleService : IArticleService
    {
        IDbContextMongoDb _dbContext { get; }
        ISettings _settings { get; }
        IMapper _mapper { get; }
        IS3Client _s3Client { get; }

        public ArticleService(IDbContextMongoDb dbContext, ISettings settings, IMapper mapper, IS3Client s3Client)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        }

        public async Task<Article> Create(CreateArticleRequest request)
        {
            request.Article.HasImage = !string.IsNullOrEmpty(request.ImageBase64);
            request.Article.HasText = !string.IsNullOrEmpty(request.Text);
            request.Article.Translations = null; // Ignore translations when creating the article

            var articleEntity = _mapper.Map<ArticleEntity>(request.Article);

            await _dbContext.Articles.InsertOneAsync(articleEntity);

            var article = _mapper.Map<Article>(articleEntity);


            // Create text file in S3
            if (!string.IsNullOrEmpty(request.Text))
            {
                var result = await UploadS3File(
                    GetTextFilename(article), 
                    request.Text
                );

                if (!result)
                {
                    // Clean up
                    await Delete(article.Id);

                    throw new Exception($"Error uploading article {request.Article.Id} to storage");
                }

                article.TextLink = GetTextLink(article);
            }

            // Create image file in S3
            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
               var result = await UploadS3File(
                    GetImageFilename(article),
                    request.ImageBase64,
                    true
                );

                if (!result)
                {
                    // Clean up
                    await Delete(request.Article.Id.ToString());

                    throw new Exception($"Error uploading image {request.Article.Id} to storage");
                }

                article.ImageLink = GetImageLink(article);
            }

            return article;
        }

        public async Task<Article> Update(UpdateArticleRequest request)
        {
            await _dbContext.Articles.UpdateOneAsync(
                Builders<ArticleEntity>.Filter.Eq(f => f.Id, new ObjectId(request.Article.Id)),
                Builders<ArticleEntity>.Update
                    .Set(f => f.Author, request.Article.Author)
                    .Set(f => f.Category, request.Article.Category)
                    .Set(f => f.Date, request.Article.Date)
                    .Set(f => f.Language, request.Article.Language)
                    .Set(f => f.Owner, request.Article.Owner)
                    .Set(f => f.SourceLink, request.Article.SourceLink)
                    .Set(f => f.Subtitle, request.Article.Subtitle)
                    .Set(f => f.Summary, request.Article.Summary)
                    .Set(f => f.Title, request.Article.Title)
                    .Set(f => f.HasText, request.Article.HasText)
                    .Set(f => f.HasImage, request.Article.HasImage)
            );

            return request.Article;
        }

        public async Task UpdateText(UpdateArticleTextRequest request)
        {
            await _dbContext.Articles.UpdateOneAsync(
                Builders<ArticleEntity>.Filter.Eq(f => f.Id, new ObjectId(request.Id)),
                Builders<ArticleEntity>.Update
                    .Set(f => f.HasText, true)
            );

            await UploadS3File(
                GetTextFilename(new Article { Id = request.Id }), 
                request.Text
            );
        }

        public async Task UpdateImage(UpdateArticleImageRequest request)
        {
            await _dbContext.Articles.UpdateOneAsync(
                Builders<ArticleEntity>.Filter.Eq(f => f.Id, new ObjectId(request.Id)),
                Builders<ArticleEntity>.Update
                    .Set(f => f.HasText, true)
            );

            await UploadS3File(
                GetImageFilename(new Article { Id = request.Id }),
                request.ImageBase64,
                true
            );
        }

        public async Task Delete(string id)
        {
            var article = await Get(id);

            await _dbContext.Articles.DeleteOneAsync(
                Builders<ArticleEntity>.Filter.Eq(f => f.Id, new ObjectId(id))
            );

            if (article.HasText || article.HasImage || article.Translations != null)
            {
                var clientS3 = _s3Client.GetClient();

                if (article.HasText)
                {
                    // Delete text file
                    await clientS3.DeleteAsync(_settings.AWSBucket, $"{id}.txt", null);
                }

                if (article.HasImage)
                {
                    // Delete image
                    await clientS3.DeleteAsync(_settings.AWSBucket, $"{id}.jpg", null);
                }

                if (article.Translations != null)
                {
                    foreach (var translation in article.Translations)
                    {
                        // Delete text file
                        await clientS3.DeleteAsync(_settings.AWSBucket, $"{id}-{translation.Language}.txt", null);
                    };
                }
            }
        }

        public async Task<Article> Get(string id)
        {
            var result = await _dbContext.Articles.FindAsync<ArticleEntity>(
                Builders<ArticleEntity>.Filter.Eq(f => f.Id, new ObjectId(id))
            );

            var articleEntity = await result.FirstOrDefaultAsync();

            var article = _mapper.Map<Article>(articleEntity);

            if (article != null)
            {
                if (article.HasText)
                {
                    article.TextLink = GetTextLink(article);
                }

                if (article.HasImage)
                {
                    article.ImageLink = GetImageLink(article);
                }

                if (article.Translations != null)
                {
                    article.Translations.ForEach(item =>
                    {
                        if (item.HasText)
                        {
                            item.TextLink = GetTranslationTextLink(article, item);
                        }
                    });
                }

                return article;
            }
            else
            {
                throw new InvalidArgumentException(ExceptionCodes.ARTICLE_NOT_FOUND, $"Article with id {id} not found", null, StatusCodes.Status404NotFound);
            }
        }

        public async Task<ArticleSearchResponse> Search(ArticleFilter filter, int page, int pageSize)
        {
            var options = new FindOptions<ArticleEntity, ArticleEntity>
            {
                Sort = GetSortFilter(filter),
                Limit = pageSize,
                Skip = (page - 1) * pageSize
            };

            var searchFilter = GetSearchFilter(filter);

            var total = await _dbContext.Articles.CountDocumentsAsync(searchFilter);

            var result = await _dbContext.Articles.FindAsync<ArticleEntity>(
                searchFilter,
                options
            );

            var resultsEntity = await result.ToListAsync();

            var results = resultsEntity.Select(s => _mapper.Map<Article>(s)).ToList();

            results.ForEach(article =>
            {
                if (article.HasImage)
                {
                    article.TextLink = GetTextLink(article);
                }

                if (article.HasImage)
                {
                    article.ImageLink = GetImageLink(article);
                }
            });

            return new ArticleSearchResponse
            {
                Total = total,
                CurrentPage = page,
                Articles = results
            };
        }

        public SortDefinition<ArticleEntity> GetSortFilter(ArticleFilter filter)
        {
            SortDefinition<ArticleEntity> sortDefinition = null;

            if (filter != null)
            {
                if (filter.ByAuthorAsc.HasValue)
                {
                    if (filter.ByAuthorAsc.Value)
                    {
                        sortDefinition = Builders<ArticleEntity>.Sort.Ascending(a => a.Author);
                    }
                    else
                    {
                        sortDefinition = Builders<ArticleEntity>.Sort.Descending(a => a.Author);
                    }
                }

                if (filter.ByDateAsc.HasValue)
                {
                    if (filter.ByDateAsc.Value)
                    {
                        sortDefinition = Builders<ArticleEntity>.Sort.Ascending(a => a.Date);
                    }
                    else
                    {
                        sortDefinition = Builders<ArticleEntity>.Sort.Descending(a => a.Date);
                    }
                }

                if (filter.ByOwnerAsc.HasValue)
                {
                    if (filter.ByOwnerAsc.Value)
                    {
                        sortDefinition = Builders<ArticleEntity>.Sort.Ascending(a => a.Owner);
                    }
                    else
                    {
                        sortDefinition = Builders<ArticleEntity>.Sort.Descending(a => a.Owner);
                    }
                }
            }

            if (sortDefinition == null)
            {
                sortDefinition = Builders<ArticleEntity>.Sort.Ascending(a => a.Id);
            }

            return sortDefinition;
        }

        public FilterDefinition<ArticleEntity> GetSearchFilter(ArticleFilter filter)
        {
            List<FilterDefinition<ArticleEntity>> filters = new List<FilterDefinition<ArticleEntity>>();

            if (filter != null)
            {
                if (filter.Author != null)
                {
                    filters.Add(Builders<ArticleEntity>.Filter.Eq(f => f.Author, filter.Author));
                }

                if (filter.Category != null)
                {
                    filters.Add(Builders<ArticleEntity>.Filter.Eq(f => f.Category, filter.Category));
                }

                if (filter.Text != null)
                {
                    filters.Add(Builders<ArticleEntity>.Filter.Text(filter.Text, new TextSearchOptions { CaseSensitive = false, DiacriticSensitive = false }));
                }

                if (filter.From.HasValue)
                {
                    filters.Add(Builders<ArticleEntity>.Filter.Gte(f => f.Date, filter.From));
                }

                if (filter.To.HasValue)
                {
                    filters.Add(Builders<ArticleEntity>.Filter.Lte(f => f.Date, filter.To));
                }

                if (filter.Language != null)
                {
                    filters.Add(Builders<ArticleEntity>.Filter.Eq(f => f.Language, filter.Language));
                }

                if (filter.Owner != null)
                {
                    filters.Add(Builders<ArticleEntity>.Filter.Eq(f => f.Owner, filter.Owner));
                }

                if (filter.TranslationLanguage != null)
                {
                    filters.Add(Builders<ArticleEntity>.Filter.ElemMatch(f => f.Translations, x => x.Language == filter.TranslationLanguage));
                }

                if (filter.PublishedBy != null)
                {
                    filters.Add(Builders<ArticleEntity>.Filter.ElemMatch(f => f.Translations, x => x.Publications.Any(a => a.Publisher == filter.PublishedBy)));
                }
            }

            if (filters.Count > 0)
            {
                return Builders<ArticleEntity>.Filter.And(filters);
            }
            else
            {
                return Builders<ArticleEntity>.Filter.Empty;
            }
        }

        private async Task<bool> UploadS3File(string filename, string content, bool isBase64 = false)
        {
            PutObjectResponse response;
            var clientS3 = _s3Client.GetClient();

            if (isBase64)
            {
                using MemoryStream imageData = new MemoryStream();
                var bytes = Convert.FromBase64String(content);
                imageData.Write(bytes, 0, bytes.Length);
                imageData.Flush();

                response = await clientS3.PutObjectAsync(
                    new PutObjectRequest
                    {
                        BucketName = _settings.AWSBucket,
                        Key = filename,
                        InputStream = imageData,
                    }
                );
            }
            else
            {
                response = await clientS3.PutObjectAsync(
                    new PutObjectRequest
                    {
                        BucketName = _settings.AWSBucket,
                        Key = filename,
                        ContentBody = content,
                        ContentType = "text/html; charset=utf-8"
                    }
                );
            }

            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        private string GetTextFilename(Article article)
        {
            return $"{article.Id}.txt";
        }

        private string GetTextLink(Article article)
        {
            return $"{_settings.S3Url}/{article.Id}.txt";
        }

        private string GetTranslationTextLink(Article article, Translation translation)
        {
            return $"{_settings.S3Url}/{article.Id}-{translation.Language}.txt";
        }

        private string GetImageFilename(Article article)
        {
            return $"{article.Id}.jpg";
        }

        private string GetImageLink(Article article)
        {
            return $"{_settings.S3Url}/{article.Id}.jpg";
        }
    }
}
