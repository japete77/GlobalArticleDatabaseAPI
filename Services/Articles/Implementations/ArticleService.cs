using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Config.Interfaces;
using DataAccess.DbContext.MongoDB.Interfaces;
using GlobalArticleDatabase.Services.Articles.Interfaces;
using GlobalArticleDatabaseAPI.Models;
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

        public ArticleService(IDbContextMongoDb dbContext, ISettings settings)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task<Article> Create(CreateArticleRequest request)
        {
            await _dbContext.Articles.InsertOneAsync(request.Article);

            var credentials = new BasicAWSCredentials(_settings.AWSAccessKey, _settings.AWSSecretKey);

            IAmazonS3 clientS3 = new AmazonS3Client(
                credentials,
                RegionEndpoint.EUWest1
            );

            // Create text file in S3
            if (!string.IsNullOrEmpty(request.Text))
            {
                var textFile = $"{request.Article.Id}-{request.Article.Language}.txt";
                var responseTextFile = await clientS3.PutObjectAsync(
                    new PutObjectRequest
                    {
                        BucketName = _settings.AWSBucket,
                        Key = textFile,
                        ContentBody = request.Text
                    }
                );

                if (responseTextFile.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    // Clean up
                    await Delete(request.Article.Id.ToString());

                    throw new Exception($"Error uploading article {request.Article.Id} to storage");
                }

                request.Article.TextLink = $"{_settings.S3Url}/{textFile}";
            }

            // Create image file in S3
            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                using MemoryStream imageData = new MemoryStream();
                var bytes = Convert.FromBase64String(request.ImageBase64);
                imageData.Write(bytes, 0, bytes.Length);
                imageData.Flush();

                var imageFile = $"{request.Article.Id}.jpg";
                var responseImageFile = await clientS3.PutObjectAsync(
                    new PutObjectRequest
                    {
                        BucketName = _settings.AWSBucket,
                        Key = imageFile,
                        InputStream = imageData
                    }
                );

                if (responseImageFile.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    // Clean up
                    await Delete(request.Article.Id.ToString());

                    throw new Exception($"Error uploading image {request.Article.Id} to storage");
                }

                request.Article.HasImage = true;
                request.Article.ImageLink = $"{_settings.S3Url}/{imageFile}";
            }

            return request.Article;
        }

        public async Task Delete(string id)
        {
            await _dbContext.Articles.DeleteOneAsync(
                Builders<Article>.Filter.Eq(f => f.Id, new ObjectId(id))
            );

            var credentials = new BasicAWSCredentials(_settings.AWSAccessKey, _settings.AWSSecretKey);

            IAmazonS3 clientS3 = new AmazonS3Client(
                credentials,
                RegionEndpoint.EUWest1
            );

            // TODO: Delete text files

            // Delete image
            await clientS3.DeleteAsync(_settings.AWSBucket, $"{id}.jpg", null);
        }

        public async Task<Article> Get(string id)
        {
            var result = await _dbContext.Articles.FindAsync<Article>(
                Builders<Article>.Filter.Eq(f => f.Id, new ObjectId(id))
            );

            var article = await result.FirstOrDefaultAsync();

            if (article != null)
            {
                article.TextLink = $"{_settings.S3Url}/{article.Id}-{article.Language}.txt";

                if (article.HasImage)
                {
                    article.ImageLink = $"{_settings.S3Url}/{article.Id}.jpg";
                }
            }

            return article;
        }

        public async Task<ArticleSearchResponse> Search(ArticleFilter filter, int page, int pageSize)
        {
            var options = new FindOptions<Article, Article>
            {
                Sort = GetSortFilter(filter),
                Limit = pageSize,
                Skip = (page - 1) * pageSize
            };

            var searchFilter = GetSearchFilter(filter);

            var total = await _dbContext.Articles.CountDocumentsAsync(searchFilter);

            var result = await _dbContext.Articles.FindAsync<Article>(
                searchFilter,
                options
            );

            var results = await result.ToListAsync();

            results.ForEach(article =>
            {
                article.TextLink = $"{_settings.S3Url}/{article.Id}-{article.Language}.txt";
                if (article.HasImage)
                {
                    article.ImageLink = $"{_settings.S3Url}/{article.Id}.jpg";
                }
            });

            return new ArticleSearchResponse
            {
                Total = total,
                CurrentPage = page,
                Articles = results
            };
        }

        public SortDefinition<Article> GetSortFilter(ArticleFilter filter)
        {
            SortDefinition<Article> sortDefinition = null;

            if (filter != null)
            {
                if (filter.ByAuthorAsc.HasValue)
                {
                    if (filter.ByAuthorAsc.Value)
                    {
                        sortDefinition = Builders<Article>.Sort.Ascending(a => a.Author);
                    }
                    else
                    {
                        sortDefinition = Builders<Article>.Sort.Descending(a => a.Author);
                    }
                }

                if (filter.ByDateAsc.HasValue)
                {
                    if (filter.ByDateAsc.Value)
                    {
                        sortDefinition = Builders<Article>.Sort.Ascending(a => a.Date);
                    }
                    else
                    {
                        sortDefinition = Builders<Article>.Sort.Descending(a => a.Date);
                    }
                }

                if (filter.ByOwnerAsc.HasValue)
                {
                    if (filter.ByOwnerAsc.Value)
                    {
                        sortDefinition = Builders<Article>.Sort.Ascending(a => a.Owner);
                    }
                    else
                    {
                        sortDefinition = Builders<Article>.Sort.Descending(a => a.Owner);
                    }
                }
            }

            if (sortDefinition == null)
            {
                sortDefinition = Builders<Article>.Sort.Ascending(a => a.Id);
            }

            return sortDefinition;
        }

        public FilterDefinition<Article> GetSearchFilter(ArticleFilter filter)
        {
            List<FilterDefinition<Article>> filters = new List<FilterDefinition<Article>>();

            if (filter != null)
            {
                if (filter.Author != null)
                {
                    filters.Add(Builders<Article>.Filter.Eq(f => f.Author, filter.Author));
                }

                if (filter.Category != null)
                {
                    filters.Add(Builders<Article>.Filter.Eq(f => f.Category, filter.Category));
                }

                if (filter.Title != null)
                {
                    filters.Add(Builders<Article>.Filter.Text(filter.Title, new TextSearchOptions { CaseSensitive = false, DiacriticSensitive = false }));
                }

                if (filter.From.HasValue)
                {
                    filters.Add(Builders<Article>.Filter.Gte(f => f.Date, filter.From));
                }

                if (filter.To.HasValue)
                {
                    filters.Add(Builders<Article>.Filter.Lte(f => f.Date, filter.To));
                }

                if (filter.Language != null)
                {
                    filters.Add(Builders<Article>.Filter.Eq(f => f.Language, filter.Language));
                }
                if (filter.Owner != null)
                {
                    filters.Add(Builders<Article>.Filter.Eq(f => f.Owner, filter.Owner));
                }

            }

            if (filters.Count > 0)
            {
                return Builders<Article>.Filter.And(filters);
            }
            else
            {
                return Builders<Article>.Filter.Empty;
            }
        }
    }
}
