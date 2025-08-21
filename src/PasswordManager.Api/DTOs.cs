namespace PasswordManager.Api
{
    public class DTOs
    {
        public record InitRequest(string MasterPassword, string vaultName);
        public record UnlockRequest(string MasterPassword, string vaultName);
        public record LockRequest(string SessionToken);
    }
}
