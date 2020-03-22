using GlobalArticleDatabaseAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlobalArticleDatabase.Services.Articles.Interfaces
{
    public interface IArticleService
    {
        Task<Article> Create(CreateArticleRequest request);
        Task Delete(string id);
        Task<Article> Get(string id);
        Task<ArticleSearchResponse> Search(ArticleFilter filter, int page, int pageSize);
    }
}
