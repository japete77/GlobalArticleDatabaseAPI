using AutoMapper;
using GlobalArticleDatabase.DbContext.MongoDB.Interfaces;
using GlobalArticleDatabase.Repositories.Interfaces;
using GlobalArticleDatabaseAPI.DbContext.Models;
using GlobalArticleDatabaseAPI.Models;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalArticleDatabase.DataAccess.Repositories.MongoDB
{
    public class AuthRenewRepositoryMongoDb : IAuthRenewRepository
    {
        private IDbContextMongoDb _dbContext { get; }
        private IMapper _mapper { get; }

        public AuthRenewRepositoryMongoDb(IDbContextMongoDb dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task DeleteAsync(string renewToken, CancellationToken cancellationToken = default)
        {
            await _dbContext.RenewTokens.FindOneAndDeleteAsync<AuthRenewEntity>(
                    Builders<AuthRenewEntity>.Filter.Eq(s => s.RenewToken, renewToken),
                    null,
                    cancellationToken
                );
        }

        public async Task DeleteByUserTokenAsync(string userToken, CancellationToken cancellationToken = default)
        {
            await _dbContext.RenewTokens.FindOneAndDeleteAsync<AuthRenewEntity>(
                    Builders<AuthRenewEntity>.Filter.Eq(s => s.UserToken, userToken),
                    null,
                    cancellationToken
                );
        }

        public AuthRenew GetByUserToken(string userToken)
        {
            var result = _dbContext.RenewTokens.Find<AuthRenewEntity>(
                    Builders<AuthRenewEntity>.Filter.Eq(s => s.UserToken, userToken),
                    null
                );

            var authRenew = result.SingleOrDefault();

            return _mapper.Map<AuthRenew>(authRenew);
        }

        public async Task<AuthRenew> GetAsync(string renewToken, CancellationToken cancellationToken = default)
        {
            var result = await _dbContext.RenewTokens.FindAsync<AuthRenewEntity>(
                    Builders<AuthRenewEntity>.Filter.Eq(s => s.RenewToken, renewToken),
                    null,
                    cancellationToken
                );


            var authRenew = await result.SingleOrDefaultAsync();

            return _mapper.Map<AuthRenew>(authRenew);
        }

        public async Task InsertAsync(AuthRenew authRenew, CancellationToken cancellationToken = default)
        {
            await _dbContext.RenewTokens.InsertOneAsync(
                _mapper.Map<AuthRenewEntity>(authRenew),
                null,
                cancellationToken
            );
        }
    }
}
