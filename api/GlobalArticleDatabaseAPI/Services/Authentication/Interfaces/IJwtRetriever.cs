using System.IdentityModel.Tokens.Jwt;

namespace GlobalArticleDatabase.Services.Authentication.Interfaces
{
    public interface IJwtRetriever
    {
        JwtSecurityToken Get();
    }
}
