using CloudStoragePlatform.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Domain.RepositoryContracts
{
    public interface IUserSessionsRepository
    {
        Task<UserSession> AddSessionAsync(UserSession session);
        Task RemoveSessionAsync(UserSession session);
        Task<UserSession?> GetByRefreshTokenAsync(string refreshToken);
        Task<IEnumerable<UserSession>> GetByUserIdAsync(Guid userId);
        Task SaveChangesAsync();
    }
} 