using PasswordManager.Services.Interfaces;
using System.Collections.Concurrent;

namespace PasswordManager.Services.Implementations
{
    public class SessionManager : ISessionManager
    {
        private class SessionEntry
        {
            public byte[] VaultKey { get; }
            public DateTime Expiry { get; }

            public SessionEntry(byte[] vaultKey, TimeSpan ttl)
            {
                VaultKey = vaultKey;
                Expiry = DateTime.UtcNow.Add(ttl);
            }

            public bool IsExpired => DateTime.UtcNow > Expiry;
        }

        private readonly ConcurrentDictionary<string, SessionEntry> _sessions = new();

        public string CreateSession(byte[] vaultKey, TimeSpan ttl)
        {
            var token = Guid.NewGuid().ToString("N"); // unique token
            var entry = new SessionEntry(vaultKey, ttl);

            _sessions[token] = entry;
            return token;
        }

        public void RemoveSession(string sessionToken)
        {
            _sessions.TryRemove(sessionToken, out _);
        }

        public byte[]? GetKey(string sessionToken)
        {
            if (_sessions.TryGetValue(sessionToken, out var entry))
            {
                if (!entry.IsExpired)
                {
                    return entry.VaultKey;
                }

                // Expired → cleanup
                _sessions.TryRemove(sessionToken, out _);
            }

            return null;
        }
    }
}
