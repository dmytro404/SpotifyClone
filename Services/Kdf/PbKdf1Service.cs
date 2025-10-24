using System.Security.Cryptography;
using System.Text;

namespace SpotifyClone.Services.Kdf
{
    // Implementation by RFC 8018
    public class PbKdf2Service : IKdfService
    {
        private const int Iterations = 100_000; 
        private const int KeyLength = 32;

        public string Dk(string password, string salt)
        {
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
            byte[] key = pbkdf2.GetBytes(KeyLength);
            return Convert.ToHexString(key);
        }
    }
}