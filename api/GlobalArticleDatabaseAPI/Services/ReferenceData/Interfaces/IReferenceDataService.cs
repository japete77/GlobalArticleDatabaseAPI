using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Services.ReferenceData.Interfaces
{
    public interface IReferenceDataService
    {
        Task<List<string>> GetAuthors();
        Task<List<string>> GetCategories();
        Task<List<string>> GetTopics();
        Task<List<string>> GetOwners();
        Task<List<string>> GetTranslationLanguages();
    }
}
