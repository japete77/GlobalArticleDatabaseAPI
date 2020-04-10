using System.Collections.Generic;
using System.Security.Claims;

namespace GlobalArticleDatabaseAPI.Services.Authentication.Interfaces
{
    public interface IJwtGenerator
    {
        string Create(IEnumerable<Claim> claims);
    }
}
