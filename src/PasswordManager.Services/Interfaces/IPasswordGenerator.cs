namespace PasswordManager.Services.Interfaces
{
    public interface IPasswordGenerator
    {
        string GenerateSecurePassword(int length = 16);
    }
}
