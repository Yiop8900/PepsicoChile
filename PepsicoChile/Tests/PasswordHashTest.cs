using System.Security.Cryptography;
using System.Text;

namespace PepsicoChile.Tests
{
/// <summary>
    /// Clase de ayuda para probar el hashing de contraseñas
    /// </summary>
    public class PasswordHashTest
    {
        public static string HashPassword(string password)
   {
 using (var sha256 = SHA256.Create())
            {
       var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
             return Convert.ToBase64String(hashedBytes);
  }
        }

        /// <summary>
   /// Método para probar hashes de contraseñas
        /// </summary>
    public static void TestPasswordHashes()
      {
  Console.WriteLine("=== Test de Hash de Contraseñas ===");
      Console.WriteLine();

      var testPasswords = new[] { "123456", "password", "admin", "test123" };

          foreach (var password in testPasswords)
        {
     var hash = HashPassword(password);
    Console.WriteLine($"Password: {password}");
     Console.WriteLine($"Hash SHA256: {hash}");
    Console.WriteLine();
     }

            // Verificar hash específico de 123456
var expectedHash = "jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=";
    var actualHash = HashPassword("123456");
     
            Console.WriteLine("=== Verificación de Hash ===");
      Console.WriteLine($"Hash esperado:  {expectedHash}");
       Console.WriteLine($"Hash generado:  {actualHash}");
            Console.WriteLine($"¿Coinciden?: {expectedHash == actualHash}");
        }
    }
}
