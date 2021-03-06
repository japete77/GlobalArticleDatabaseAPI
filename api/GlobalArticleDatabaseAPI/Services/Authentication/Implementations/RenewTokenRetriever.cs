﻿using GlobalArticleDatabaseAPI.Repositories.Interfaces;
using GlobalArticleDatabaseAPI.Services.Authentication.Interfaces;
using GlobalArticleDatabaseAPI.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Services.Authentication.Implementations
{
    public class RenewTokenRetriever : IRenewTokenRetriever
    {
        IAuthRenewRepository _authRenewRepository { get; }

        public RenewTokenRetriever(IAuthRenewRepository authRenewRepository)
        {
            _authRenewRepository = authRenewRepository ?? throw new ArgumentNullException(nameof(authRenewRepository));
        }

        public async Task<AuthRenew> GetAsync(string renewToken, CancellationToken cancellationToken = default)
        {
            return await _authRenewRepository.GetAsync(renewToken, cancellationToken);
        }
    }
}
