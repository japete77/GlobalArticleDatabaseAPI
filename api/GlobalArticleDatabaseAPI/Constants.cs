using Config.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GlobalArticleDatabase
{
    public static class Constants
    {
        public static class Swagger
        {
            public static string EndPoint => $"/swagger/{Version}/swagger.json";
            public const string ApiName = "Global Article Database API";
            public const string Version = "v1";
        }

        public static class App
        {
            public const string MasterTenant = "master";
            public const string LogsDatabase = "logs";
            public const int StreamBufferSize = 8192;
            public const int TokenExpirationInMinutes = 60;
            public const int ResetPasswordTokenExpirationInMinutes = 1440;
            public const int RenewTokenPasswordExpirationInMinutes = 120;
        }

        public static class Roles
        {
            public const string Admin = "ADMIN";
            public const string Editor = "EDITOR";
            public const string Reader = "READER";
        }

        public static class DefaultAdminUser
        {
            public const string Username = "admin";
            public const string NormalizedUsername = "ADMIN";
            public const string PasswordHash = "AQAAAAEAACcQAAAAEIXJAOSQCp6YeyR22WVl97+EKBy2cfKt67kdXOUVACkN4khAIy6UUNiwGSTtL/SLDQ==";
        }

        public static class Security
        {
            public const string SecurityDocResource = "GlobalArticleDatabase.Security.json";
            public const string Anonymous = "Anonymous";
        }


        public static class Authentication
        {
            public static TokenValidationParameters tokenValidationParameters => new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Startup.GetService<ISettings>().Secret))
            };

            public const string AuthorizationHeaderKey = "Authorization";
            public const string BearerType = "Bearer";
        }
    }
}
