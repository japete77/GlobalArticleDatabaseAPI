using GlobalArticleDatabaseAPI.Models;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Services.Articles.Interfaces
{
    public interface IPublicationService
    {
        Task Create(CreatePublicationRequest request);
        Task Delete(string articleId, string language, string publisher);
    }
}
