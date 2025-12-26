    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    namespace SwordfishNet_Unified
    {
        internal class EncryptionHelper
        {
        private static readonly byte[] Salt = Encoding.UTF8.GetBytes("Y7W0xXp_o8zM5gR9-KqN-u2JtV1jA6bDcSfL3eH4cEwI7hG_rZlP8yFv4sQ_aT0bU9d"); // Strong unique random salt
        private const int KeySize = 256; // 256 bit encryption
        private const int Iterations = 100000; // Number of iterations for PBKDF2 to render brute-force attacks more difficult

        //Encrypt data and save to local file so the information can be recycled at app relaunch.
        public static void EncryptAndSave(string password, string dataToEncrypt, string filePath)
        {
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                password,
                Salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize / 8); // Derive a 256-bit key

            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV(); // Generate a new IV for each encryption

            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length); // Prepend IV

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV)) // Create encryptor
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) // Create crypto stream
                                                                                     // Encrypt the data and write to memory stream
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(dataToEncrypt);
                cs.Write(dataBytes, 0, dataBytes.Length);
                cs.FlushFinalBlock();
            }

            File.WriteAllBytes(filePath, ms.ToArray()); // Save encrypted data to file
        }

        // Decrypt data from local file using the provided password
        public static string? DecryptAndLoad(string password, string filePath)
            {
                if (!File.Exists(filePath)) // Check if file exists
                return null; // Return null if file does not exist
            try // Try to decrypt the data
            {
                    byte[] encryptedDataWithIv = File.ReadAllBytes(filePath);
                    byte[] iv = new byte[16];

                    Buffer.BlockCopy(encryptedDataWithIv, 0, iv, 0, iv.Length); // Extract IV

                int encryptedDataLength = encryptedDataWithIv.Length - iv.Length; // Calculate length of encrypted data
                byte[] encryptedData = new byte[encryptedDataLength]; // Create array for encrypted data
                Buffer.BlockCopy(encryptedDataWithIv, iv.Length, encryptedData, 0, encryptedDataLength); // Extract encrypted data

                byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    Salt,
                    Iterations,
                    HashAlgorithmName.SHA256,
                    KeySize / 8); // Derive the same 256-bit key

                using var aes = Aes.Create(); // Create AES instance
                aes.Key = key;
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV); // Create decryptor
                using var ms = new MemoryStream(encryptedData); // Create memory stream with encrypted data
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read); // Create crypto stream for decryption
                using var sr = new StreamReader(cs); // Create stream reader to read decrypted 
                return sr.ReadToEnd(); // Return decrypted data as string
            }
                catch (CryptographicException) // Handle decryption errors (e.g., wrong password)
            {
                    return null;
                }
                catch (Exception ex) // Handle other exceptions
            {
                    System.Diagnostics.Debug.WriteLine($"Decryption Error: {ex.Message}");
                    return null;
                }
            }
        }
    }