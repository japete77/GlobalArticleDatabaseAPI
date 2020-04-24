using GlobalArticleDatabaseAPI.Models;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Services.Articles.Interfaces
{
    public interface IArticleService
    {
        Task<Article> Create(CreateArticleRequest request);
        Task<Article> Update(UpdateArticleRequest request);
        Task<string> GetText(string id, string language = null);
        Task UpdateText(UpdateArticleTextRequest request);
        Task Delete(string id);
        Task<Article> Get(string id);
        Task<ArticleSearchResponse> Search(ArticleFilter filter, int page, int pageSize);
        Task<long> SearchCount(ArticleFilter filter);
    }
}
