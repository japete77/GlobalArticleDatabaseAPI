using GlobalArticleDatabaseAPI.Models;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Services.Email.Interfaces
{
    public interface IEmailSender
    {
        Task Send(EmailMessage message);
    }
}
