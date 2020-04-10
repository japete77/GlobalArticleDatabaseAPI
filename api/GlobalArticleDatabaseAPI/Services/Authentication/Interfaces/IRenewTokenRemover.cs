﻿using System.Threading;
using System.Threading.Tasks;

namespace GlobalArticleDatabase.Services.Authentication.Interfaces
{
    public interface IRenewTokenRemover
    {
        Task DeleteAsync(string renewToken, CancellationToken cancellationToken = default);
        Task DeleteByUserTokenAsync(string userToken, CancellationToken cancellationToken = default);
    }
}
