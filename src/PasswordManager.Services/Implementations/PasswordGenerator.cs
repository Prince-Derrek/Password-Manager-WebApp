using System.Security.Cryptography;
using PasswordManager.Services.Interfaces;

namespace PasswordManager.Services.Implementations
{
    public class PasswordGenerator : IPasswordGenerator
    {
        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Digits = "0123456789";
        private const string Special = "!@#$%^&*()-_=+<>?";
        private static readonly string AllChars = Uppercase + Lowercase + Digits + Special;

        public string GenerateSecurePassword(int length = 16)
        {
            if (length < 4)
                throw new ArgumentException("Password length must be at least 4 to include all character types.");

            var passwordChars = new List<char>();

            // Ensure at least one of each
            passwordChars.Add(GetRandomChar(Uppercase));
            passwordChars.Add(GetRandomChar(Lowercase));
            passwordChars.Add(GetRandomChar(Digits));
            passwordChars.Add(GetRandomChar(Special));

            // Fill the rest with random from all
            for (int i = passwordChars.Count; i < length; i++)
            {
                passwordChars.Add(GetRandomChar(AllChars));
            }

            // Shuffle the characters for randomness
            return Shuffle(passwordChars);
        }

        private static char GetRandomChar(string chars)
        {
            using var rng = RandomNumberGenerator.Create();
            var randomByte = new byte[1];
            rng.GetBytes(randomByte);
            int index = randomByte[0] % chars.Length;
            return chars[index];
        }

        private static string Shuffle(List<char> chars)
        {
            using var rng = RandomNumberGenerator.Create();
            int n = chars.Count;
            while (n > 1)
            {
                var box = new byte[1];
                do { rng.GetBytes(box); }
                while (!(box[0] < n * (Byte.MaxValue / n)));

                int k = box[0] % n;
                n--;
                (chars[k], chars[n]) = (chars[n], chars[k]);
            }
            return new string(chars.ToArray());
        }
    }
}
