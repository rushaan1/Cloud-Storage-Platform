using System;
using System.Collections.Generic;

namespace CloudStoragePlatform.Core.Services
{
    public class UserBasicInfo
    {
        private readonly Dictionary<Guid, float> _spaceUsedByUsers = new();
        private readonly Dictionary<Guid, string> _userPersonNames = new();

        public void SetUserSpaceUsed(Guid userId, float spaceInMB)
        {
            lock (_spaceUsedByUsers)
            {
                _spaceUsedByUsers[userId] = spaceInMB;
            }
        }

        public float? GetUserSpaceUsed(Guid userId)
        {
            lock (_spaceUsedByUsers)
            {
                return _spaceUsedByUsers.TryGetValue(userId, out var value) ? value : null;
            }
        }

        public void SetUserPersonName(Guid userId, string personName)
        {
            lock (_userPersonNames)
            {
                _userPersonNames[userId] = personName;
            }
        }

        public string? GetUserPersonName(Guid userId)
        {
            lock (_userPersonNames)
            {
                return _userPersonNames.TryGetValue(userId, out var value) ? value : null;
            }
        }
    }
}

