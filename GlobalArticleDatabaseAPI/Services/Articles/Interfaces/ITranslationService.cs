using GlobalArticleDatabaseAPI.Models;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Services.Articles.Interfaces
{
    public interface ITranslationService
    {
        Task Create(CreateTranslationRequest request);
        Task Update(UpdateTranslationRequest request);
        Task UpdateText(UpdateTranslationTextRequest request);
        Task Delete(string articleId, string language);
    }
}
