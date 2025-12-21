    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    namespace SwordfishNet_Unified
    {
        internal class EncryptionHelper
        {
            private static readonly byte[] Salt = Encoding.UTF8.GetBytes("Y7W0xXp_o8zM5gR9-KqN-u2JtV1jA6bDcSfL3eH4cEwI7hG_rZlP8yFv4sQ_aT0bU9d"); // **CHANGE THIS TO A STRONG, UNIQUE VALUE**
            private const int KeySize = 256;
            private const int Iterations = 10000;
        public static void EncryptAndSave(string password, string dataToEncrypt, string filePath)
        {
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                password,
                Salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize / 8);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length); // Prepend IV

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(dataToEncrypt);
                cs.Write(dataBytes, 0, dataBytes.Length);
                cs.FlushFinalBlock();
            }

            File.WriteAllBytes(filePath, ms.ToArray());
        }
        public static string? DecryptAndLoad(string password, string filePath)
            {
                if (!File.Exists(filePath))
                    return null;
                try
                {
                    byte[] encryptedDataWithIv = File.ReadAllBytes(filePath);
                    byte[] iv = new byte[16];

                    Buffer.BlockCopy(encryptedDataWithIv, 0, iv, 0, iv.Length);

                    int encryptedDataLength = encryptedDataWithIv.Length - iv.Length;
                    byte[] encryptedData = new byte[encryptedDataLength];
                    Buffer.BlockCopy(encryptedDataWithIv, iv.Length, encryptedData, 0, encryptedDataLength);

                byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    Salt,
                    Iterations,
                    HashAlgorithmName.SHA256,
                    KeySize / 8);

                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream(encryptedData);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                // 4. Decrypt the data
                return sr.ReadToEnd();
            }
                catch (CryptographicException)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Decryption Error: {ex.Message}");
                    return null;
                }
            }
        }
    }