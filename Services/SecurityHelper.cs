using System.Security.Cryptography;
using System.Text;

namespace SHL_Platform.Services
{
    public class SecurityHelper
    {
        public static string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder sb = new();
            using (RNGCryptoServiceProvider rng = new())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    sb.Append(validChars[(int)(num % (uint)validChars.Length)]);
                }
            }
            return sb.ToString();
        }

        public static string HashPassword(string password)
        {
            // You can use a secure hashing algorithm like bcrypt
            // For simplicity, here's how you can hash with SHA256
            byte[] hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            StringBuilder sb = new();
            foreach (byte b in hashedBytes)
            {
                sb.Append(b.ToString("x2")); // Convert byte to hexadecimal
            }
            return sb.ToString();
        }
    }
}
