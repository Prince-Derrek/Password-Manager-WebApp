using System;

namespace PasswordManager.Services.Interfaces;

public interface ISessionManager
{
    string CreateSession(byte[] vaultKey, TimeSpan ttl);
    void RemoveSession(string sessionToken);
    byte[]? GetKey(string sessionToken);
}
