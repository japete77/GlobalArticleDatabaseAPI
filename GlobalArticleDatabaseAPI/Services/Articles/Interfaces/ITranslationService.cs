using GlobalArticleDatabaseAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Services.Articles.Interfaces
{
    public interface ITranslationService
    {
        Task Create(string articleId, Translation translation);
        Task Update(string articleId, Translation translation);
        Task Delete(string articleId, string language);
    }
}
