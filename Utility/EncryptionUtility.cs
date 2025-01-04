using System.Security.Cryptography;
using System.Text;

namespace EncryptMC.Utility
{
    internal static class EncryptionUtility
    {
        internal static string GenerateContentKey()
        {
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var contentKey = new StringBuilder(32);

            byte[] buffer = new byte[1];
            for (int i = 0; i < 32; i++)
            {
                RandomNumberGenerator.Fill(buffer);
                char randomChar = characters[buffer[0] % characters.Length];
                contentKey.Append(randomChar);
            }

            return contentKey.ToString();
        }

        internal static void WriteDataToEncryptedFile(string path, byte[] data, string contentKey)
        {
            byte[] encryptedData = EncryptData(contentKey, data);

            string? directoryPath = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directoryPath))
            {
                throw new IOException($"Failed to get directory path from file: {path}");
            }

            // Ensure output directory exists
            PathUtility.EnsureOutputFolderExists(directoryPath);

            try
            {
                // Write encrypted data to file
                File.WriteAllBytes(path, encryptedData);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to write encrypted data: {ex.Message}", ex);
            }
        }

        internal static byte[] EncryptData(string contentKey, byte[] data)
        {
            byte[] contentKeyBytes = Encoding.UTF8.GetBytes(contentKey);
            byte[] iv = contentKeyBytes[..16];
            byte[] ciphertext = Aes256Cfb8Encrypt(contentKeyBytes, iv, data);
            return ciphertext;
        }

        internal static byte[] Aes256Cfb8Encrypt(byte[] key, byte[] iv, byte[] data)
        {
            using Aes aes = Aes.Create();
            aes.Mode = CipherMode.CFB;
            aes.Padding = PaddingMode.None;
            aes.BlockSize = 128; // Explicitly set block size to 128 bits (standard for AES)
            aes.KeySize = 256;   // Explicitly set key size to 256 bits (standard for AES-256)

            using ICryptoTransform aesEncryptor = aes.CreateEncryptor(key, iv);
            using MemoryStream msEncrypt = new();
            using CryptoStream csEncrypt = new(msEncrypt, aesEncryptor, CryptoStreamMode.Write);

            csEncrypt.Write(data, 0, data.Length);

            // Ensure padding to nearest block size (CFB8 mode needs this for alignment)
            int paddingNeeded = aes.BlockSize / 8 - (data.Length % (aes.BlockSize / 8));
            if (paddingNeeded > 0)
            {
                csEncrypt.Write(new byte[paddingNeeded], 0, paddingNeeded);
            }

            csEncrypt.FlushFinalBlock();
            return msEncrypt.ToArray();
        }
    }
}
