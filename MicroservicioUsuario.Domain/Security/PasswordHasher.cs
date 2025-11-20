using System;
using System.Security.Cryptography;
using System.Text;

namespace MicroservicioUsuario.Domain.Security
{
    /// <summary>
    /// Utilidad para hashear y verificar contraseñas usando PBKDF2
    /// Formato: PBKDF2:iteraciones:salt:hash
    /// </summary>
    public static class PasswordHasher
    {
   private const int ITERATIONS = 100000; // 100,000 iteraciones (recomendado por OWASP)
    private const int SALT_SIZE = 16; // 16 bytes de salt
        private const int HASH_SIZE = 32; // 32 bytes de hash

     /// <summary>
    /// Hashea una contraseña usando PBKDF2
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <returns>String con formato PBKDF2:iteraciones:salt:hash</returns>
        public static string HashPassword(string password)
        {
          if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

            // Generar salt aleatorio
       byte[] salt = new byte[SALT_SIZE];
      using (var rng = RandomNumberGenerator.Create())
{
      rng.GetBytes(salt);
  }

            // Generar hash usando PBKDF2
 using (var pbkdf2 = new Rfc2898DeriveBytes(
     password,
      salt,
      ITERATIONS,
     HashAlgorithmName.SHA256))
    {
      byte[] hash = pbkdf2.GetBytes(HASH_SIZE);

   // Formato: PBKDF2:iteraciones:salt(base64):hash(base64)
    string result = $"PBKDF2:{ITERATIONS}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
          return result;
     }
        }

        /// <summary>
   /// Verifica si una contraseña coincide con un hash
     /// </summary>
        /// <param name="password">Contraseña en texto plano a verificar</param>
     /// <param name="hashedPassword">Hash almacenado en BD</param>
     /// <returns>True si la contraseña es correcta, False en caso contrario</returns>
 public static bool VerifyPassword(string password, string hashedPassword)
        {
    if (string.IsNullOrWhiteSpace(password))
    return false;

    if (string.IsNullOrWhiteSpace(hashedPassword))
   return false;

            try
     {
        // Parsear el hash: PBKDF2:iteraciones:salt:hash
 string[] parts = hashedPassword.Split(':');
                
   if (parts.Length != 4)
       return false;

    if (parts[0] != "PBKDF2")
            return false;

   int iterations = int.Parse(parts[1]);
     byte[] salt = Convert.FromBase64String(parts[2]);
       byte[] storedHash = Convert.FromBase64String(parts[3]);

           // Generar hash con la contraseña proporcionada usando el mismo salt
        using (var pbkdf2 = new Rfc2898DeriveBytes(
        password,
  salt,
   iterations,
       HashAlgorithmName.SHA256))
         {
     byte[] computedHash = pbkdf2.GetBytes(HASH_SIZE);

       // Comparar hashes de forma segura (previene timing attacks)
     return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
      }
        }
        catch (Exception)
            {
 // Si hay algún error en el formato o conversión, retornar false
       return false;
       }
        }

        /// <summary>
        /// Verifica si un hash está en el formato correcto
   /// </summary>
        public static bool IsValidHashFormat(string hashedPassword)
        {
     if (string.IsNullOrWhiteSpace(hashedPassword))
     return false;

    string[] parts = hashedPassword.Split(':');
            return parts.Length == 4 && parts[0] == "PBKDF2";
        }

        /// <summary>
        /// Genera una contraseña temporal aleatoria
     /// </summary>
        /// <returns>Contraseña de 12 caracteres con mayúsculas, minúsculas, números y símbolos</returns>
        public static string GenerateTemporaryPassword()
     {
  const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
     const string lowercase = "abcdefghijklmnopqrstuvwxyz";
    const string digits = "0123456789";
   const string special = "@#$%&*!";
       const string allChars = uppercase + lowercase + digits + special;

            var password = new char[12];
            using (var rng = RandomNumberGenerator.Create())
      {
     // Asegurar al menos un carácter de cada tipo
              password[0] = uppercase[GetRandomInt(rng, uppercase.Length)];
    password[1] = lowercase[GetRandomInt(rng, lowercase.Length)];
   password[2] = digits[GetRandomInt(rng, digits.Length)];
      password[3] = special[GetRandomInt(rng, special.Length)];

                // Rellenar el resto aleatoriamente
                for (int i = 4; i < password.Length; i++)
     {
         password[i] = allChars[GetRandomInt(rng, allChars.Length)];
            }

                // Mezclar el array
       for (int i = password.Length - 1; i > 0; i--)
     {
           int j = GetRandomInt(rng, i + 1);
   char temp = password[i];
        password[i] = password[j];
            password[j] = temp;
                }
      }

  return new string(password);
        }

    private static int GetRandomInt(RandomNumberGenerator rng, int max)
        {
            byte[] bytes = new byte[4];
    rng.GetBytes(bytes);
            uint value = BitConverter.ToUInt32(bytes, 0);
            return (int)(value % (uint)max);
        }
}
}
