using GlobalArticleDatabaseAPI.DbContext.MongoDB.Interfaces;
using GlobalArticleDatabaseAPI.DbContext.Models;
using GlobalArticleDatabaseAPI.Services.ReferenceData.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Services.ReferenceData.Implementations
{
    public class ReferenceDataService : IReferenceDataService
    {
        IDbContextMongoDb _dbContext { get; }

        public ReferenceDataService(IDbContextMongoDb dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<List<string>> GetAuthors()
        {
            var result = await _dbContext.Articles.DistinctAsync<string>(
                "Author",
                Builders<ArticleEntity>.Filter.Empty
            );

            return await result.ToListAsync();
        }

        public async Task<List<string>> GetCategories()
        {
            var result = await _dbContext.Articles.DistinctAsync<string>(
                "Category",
                Builders<ArticleEntity>.Filter.Empty
            );

            return await result.ToListAsync();
        }

        public async Task<List<string>> GetTopics()
        {
            var result = await _dbContext.Articles.DistinctAsync<string>(
                "Topics",
                Builders<ArticleEntity>.Filter.Empty
            );

            return await result.ToListAsync();
        }

        public async Task<List<string>> GetOwners()
        {
            var result = await _dbContext.Articles.DistinctAsync<string>(
                "Owner",
                Builders<ArticleEntity>.Filter.Empty
            );

            return await result.ToListAsync();
        }
    }
}
