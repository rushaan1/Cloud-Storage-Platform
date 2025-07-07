using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core
{
    public static class SseTokenStore
    {
        private static readonly ConcurrentDictionary<string, (string UserId, DateTime Expiry)> _tokens = new();

        public static void Save(string token, string userId, TimeSpan ttl)
        {
            _tokens[token] = (userId, DateTime.UtcNow.Add(ttl));
        }

        public static string? GetAndRemove(string token)
        {
            if (_tokens.TryRemove(token, out var entry) && entry.Expiry > DateTime.UtcNow)
                return entry.UserId;

            return null;
        }
    }

}
