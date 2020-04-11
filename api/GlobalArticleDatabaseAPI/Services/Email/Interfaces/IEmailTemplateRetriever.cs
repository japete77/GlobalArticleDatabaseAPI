namespace GlobalArticleDatabaseAPI.Services.Email.Interfaces
{
    public interface IEmailTemplateRetriever
    {
        string GetResetPasswordTemplate(string name, string email, string token);
        string SetInitialPasswordTemplate(string name, string email, string token);
    }
}
