using AutoMapper;
using Core.Exceptions;
using GlobalArticleDatabaseAPI;
using GlobalArticleDatabaseAPI.DbContext.Models;
using GlobalArticleDatabaseAPI.DbContext.MongoDB.Interfaces;
using GlobalArticleDatabaseAPI.Models;
using GlobalArticleDatabaseAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalAtricleDatabaseAPI.Repositories.Implementations
{
    public class UserRepositoryMongoDb : IUserRepository
    {
        private readonly IDbContextMongoDb _dbContext;
        private IMapper _mapper { get; }

        public UserRepositoryMongoDb(
            IDbContextMongoDb dbContext,
            IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
        {
            await _dbContext.Users.FindOneAndDeleteAsync<UserEntity>(
                    Builders<UserEntity>.Filter.Eq(s => s.Id, user.Id),
                    null,
                    cancellationToken
                );
        }

        public async Task<List<User>> GetAllAsync(int page, int pageSize, string filter, CancellationToken cancellationToken = default)
        {
            var options = new FindOptions<UserEntity>()
            {
                Sort = Builders<UserEntity>.Sort.Ascending(x => x.NormalizedUserName),
                Limit = pageSize,
                Skip = pageSize * (page - 1)
            };

            var result = await _dbContext.Users.FindAsync<UserEntity>(GetSearchFilter(filter), options, cancellationToken);

            var users = await result.ToListAsync();

            return users.Select(s => _mapper.Map<User>(s)).ToList();
        }

        public async Task<long> CountAsync(string filter, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users.CountDocumentsAsync(GetSearchFilter(filter), null, cancellationToken);
        }

        public long Count(string filter, CancellationToken cancellationToken = default)
        {
            return _dbContext.Users.CountDocuments(GetSearchFilter(filter), null, cancellationToken);
        }

        public async Task<User> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await _dbContext.Users.FindAsync<UserEntity>(
                    Builders<UserEntity>.Filter.Eq(s => s.Id, id),
                    null,
                    cancellationToken
                );

            var user = await result.SingleOrDefaultAsync();

            return _mapper.Map<User>(user);
        }

        public async Task<User> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            var result = await _dbContext.Users.FindAsync<UserEntity>(
                    Builders<UserEntity>.Filter.Eq(s => s.NormalizedEmail, normalizedEmail),
                    null,
                    cancellationToken
                );

            var user = await result.SingleOrDefaultAsync();

            return _mapper.Map<User>(user);
        }

        public async Task<User> GetByUserNameAsync(string normalizedUsername, CancellationToken cancellationToken = default)
        {
            var result = await _dbContext.Users.FindAsync<UserEntity>(
                    Builders<UserEntity>.Filter.Eq(s => s.NormalizedUserName, normalizedUsername),
                    null,
                    cancellationToken
                    );

            var user = await result.SingleOrDefaultAsync();

            return _mapper.Map<User>(user);
        }

        public async Task InsertAsync(User user, CancellationToken cancellationToken = default)
        {
            await _dbContext.Users.InsertOneAsync(_mapper.Map<UserEntity>(user), null, cancellationToken);
        }

        public void Insert(User user, CancellationToken cancellationToken = default)
        {
            if (!AreRolesValid(user.Roles))
            {
                throw new InvalidArgumentException(ExceptionCodes.USER_ROLES_NOT_VALID, $"Invalid role names", null, StatusCodes.Status400BadRequest);
            }

            _dbContext.Users.InsertOne(_mapper.Map<UserEntity>(user), null, cancellationToken);
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (!AreRolesValid(user.Roles))
            {
                throw new InvalidArgumentException(ExceptionCodes.USER_ROLES_NOT_VALID, $"Invalid role names", null, StatusCodes.Status400BadRequest);
            }

            var options = new FindOneAndUpdateOptions<UserEntity, UserEntity>
            {
                IsUpsert = false,
                ReturnDocument = ReturnDocument.After
            };

            var updatedUser = await _dbContext.Users.FindOneAndUpdateAsync<UserEntity>(
                        Builders<UserEntity>.Filter.Eq(s => s.Id, user.Id),
                        Builders<UserEntity>.Update
                            .Set(s => s.UserName, user.UserName)
                            .Set(s => s.NormalizedUserName, user.NormalizedUserName)
                            .Set(s => s.Email, user.Email)
                            .Set(s => s.NormalizedEmail, user.NormalizedEmail)
                            .Set(s => s.PasswordHash, user.PasswordHash)
                            .Set(s => s.ResetPasswordToken, user.ResetPasswordToken)
                            .Set(s => s.ResetPasswordTokenExpiryDate, user.ResetPasswordTokenExpiryDate)
                            .Set(s => s.Roles, user.Roles.Select(s => _mapper.Map<RoleEntity>(s)).ToList())
                            .Set(s => s.Photo, user.Photo),
                        options,
                        cancellationToken
                    );

            if (updatedUser == null)
            {
                throw new InvalidArgumentException(ExceptionCodes.USER_NOT_FOUND, $"User with id {user.Id} not found", null, StatusCodes.Status404NotFound);
            }
        }

        public async Task UpdateBasicAsync(User user, CancellationToken cancellationToken = default)
        {
            if (!AreRolesValid(user.Roles))
            {
                throw new InvalidArgumentException(ExceptionCodes.USER_ROLES_NOT_VALID, $"Invalid role names", null, StatusCodes.Status400BadRequest);
            }

            var usersSameEmail = await _dbContext.Users.CountDocumentsAsync(
                Builders<UserEntity>.Filter.Eq(s => s.NormalizedEmail, user.Email.ToUpper()) &
                Builders<UserEntity>.Filter.Ne(s => s.Id, user.Id)
            );

            if (usersSameEmail > 0)
            {
                throw new DuplicateKeyException(ExceptionCodes.IDENTITY_DUPLICATED_EMAIL, $"Email '{user.Email}' is already in use", null, StatusCodes.Status400BadRequest);
            }

            var options = new FindOneAndUpdateOptions<UserEntity, UserEntity>
            {
                IsUpsert = false,
                ReturnDocument = ReturnDocument.After
            };

            var updatedUser = await _dbContext.Users.FindOneAndUpdateAsync<UserEntity>(
                        Builders<UserEntity>.Filter.Eq(s => s.Id, user.Id),
                        Builders<UserEntity>.Update
                            .Set(s => s.Email, user.Email)
                            .Set(s => s.NormalizedEmail, user.Email.ToUpper())
                            .Set(s => s.Roles, user.Roles.Select(s => _mapper.Map<RoleEntity>(s)).ToList())
                            .Set(s => s.Photo, user.Photo),
                        options,
                        cancellationToken
                    );

            if (updatedUser == null)
            {
                throw new InvalidArgumentException(ExceptionCodes.USER_NOT_FOUND, $"User with id {user.Id} not found", null, StatusCodes.Status404NotFound);
            }
        }

        public async Task<IList<User>> GetByRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            var result = await _dbContext.Users.FindAsync(f => f.Roles != null && f.Roles.Any(a => a.Name == roleName), null, cancellationToken);

            var users = await result.ToListAsync();

            return users.Select(s => _mapper.Map<User>(s)).ToList();
        }

        private FilterDefinition<UserEntity> GetSearchFilter(string text)
        {
            // TODO: Create an index for text search and use "Builders<User>.Filter.Text(text, new TextSearchOptions() { CaseSensitive = false });"
            return string.IsNullOrEmpty(text) ? FilterDefinition<UserEntity>.Empty : Builders<UserEntity>.Filter.Where(x => x.UserName.ToLower().Contains(text.ToLower()));
        }

        private bool AreRolesValid(List<Role> roles)
        {
            if (roles == null) return true;

            List<string> validRoles = new List<string>
            {
                Constants.Roles.Admin,
                Constants.Roles.Editor,
                Constants.Roles.Reader,
            };

            return roles.Where(w => !validRoles.Contains(w.Name)).Count() == 0;
        }
    }
}
