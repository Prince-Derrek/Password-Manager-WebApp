namespace PasswordManager.Api
{
    public class DTOs
    {
        public record InitRequest(string MasterPassword);
        public record UnlockRequest(string MasterPassword);
        public record LockRequest(string SessionToken);
    }
}
