using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Infrastructure.Repositories
{
    public class UserSessionsRepository : IUserSessionsRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public UserSessionsRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserSession> AddSessionAsync(UserSession session)
        {
            await _dbContext.UserSessions.AddAsync(session);
            return session;
        }

        public async Task RemoveSessionAsync(UserSession session)
        {
            _dbContext.UserSessions.Remove(session);
        }

        public async Task<UserSession?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _dbContext.UserSessions.FirstOrDefaultAsync(s => s.RefreshToken == refreshToken);
        }

        public async Task<IEnumerable<UserSession>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.UserSessions.Where(s => s.ApplicationUserId == userId).ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
} 