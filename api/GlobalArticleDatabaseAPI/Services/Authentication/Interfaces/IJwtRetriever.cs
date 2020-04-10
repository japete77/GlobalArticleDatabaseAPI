using System.IdentityModel.Tokens.Jwt;

namespace GlobalArticleDatabaseAPI.Services.Authentication.Interfaces
{
    public interface IJwtRetriever
    {
        JwtSecurityToken Get();
    }
}
