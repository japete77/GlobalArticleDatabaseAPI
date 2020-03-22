using DataAccess.DbContext.MongoDB.Interfaces;
using GlobalArticleDatabase.Services.Articles.Interfaces;
using GlobalArticleDatabaseAPI.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalArticleDatabase.Services.Articles.Implementations
{
    public class ArticleService : IArticleService
    {
        IDbContextMongoDb _dbContext { get; }

        public ArticleService(IDbContextMongoDb dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<List<Article>> Create(List<CreateArticle> request)
        {
            var articles = request.Select(s => s.Article).ToList();
            await _dbContext.Articles.InsertManyAsync(articles);

            // Create text file

            // Create image file

            return articles;
        }

        public async Task Delete(string id)
        {
            await _dbContext.Articles.DeleteOneAsync(
                Builders<Article>.Filter.Eq(f => f.Id, new ObjectId(id))
            );

            // Delete text files

            // Delete images
        }

        public async Task<Article> Get(string id)
        {
            var result = await _dbContext.Articles.FindAsync<Article>(
                Builders<Article>.Filter.Eq(f => f.Id, new ObjectId(id))
            );

            // Add text link

            // Add image link

            return await result.FirstOrDefaultAsync();
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
