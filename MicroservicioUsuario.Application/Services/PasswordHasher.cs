using System.Security.Cryptography;
using System.Text;

namespace MicroservicioUsuario.Application.Security
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            int iterations = 100000;
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                32
            );

            return $"PBKDF2:{iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 4) return false;

            var iterations = int.Parse(parts[1]);
            var salt = Convert.FromBase64String(parts[2]);
            var hashBytes = Convert.FromBase64String(parts[3]);

            var testHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                hashBytes.Length
            );

            return CryptographicOperations.FixedTimeEquals(testHash, hashBytes);
        }
    }
}
