using Config.Interfaces;
using GlobalArticleDatabaseAPI.Helpers;
using GlobalArticleDatabaseAPI.Services.Email.Interfaces;
using System;

namespace GlobalArticleDatabaseAPI.Services.Email.Implementations
{
    public class EmailTemplateRetriever : IEmailTemplateRetriever
    {
        ISettings _settings { get; }

        public EmailTemplateRetriever(ISettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public string GetResetPasswordTemplate(string name, string email, string token)
        {
            var template = ResourcesHelper.GetResource("ResetPassword.html");

            template = template.Replace("{{name}}", name);
            template = template.Replace("{{action_url}}", $"{_settings.ResetPasswordUrl}?email={email}&token={token}");
            template = template.Replace("{{support_url}}", $"{_settings.SupportUrl}");

            return template;
        }

        public string SetInitialPasswordTemplate(string name, string email, string token)
        {
            var template = ResourcesHelper.GetResource("SetInitialPassword.html");

            template = template.Replace("{{name}}", name);
            template = template.Replace("{{action_url}}", $"{_settings.ResetPasswordUrl}?email={email}&token={token}");
            template = template.Replace("{{support_url}}", $"{_settings.SupportUrl}");

            return template;
        }
    }
}
