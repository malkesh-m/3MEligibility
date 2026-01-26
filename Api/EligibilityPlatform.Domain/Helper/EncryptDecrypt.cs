using System.Security.Cryptography;
using System.Text;

namespace MEligibilityPlatform.Domain.Helper
{
    public static class EncryptDecrypt
    {
        //private readonly string _key;

        //public EncryptDecrypt(IConfiguration configuration)
        //{
        //    _key = configuration["EncryptionSettings:Key"];
        //}

        //private readonly string _key;

        //public EncryptDecrypt(IConfiguration configuration)
        //{
        //    _key = configuration["EncryptionSettings:Key"];
        //}

        /// <summary>
        /// Encrypts the specified plain text using the provided key.
        /// </summary>
        /// <param name="plainText">The text to be encrypted.</param>
        /// <param name="key">The encryption key. Must be a valid AES key length (e.g., 16, 24, or 32 characters).</param>
        /// <returns>The encrypted string, encoded in Base64 format.</returns>
        public static string EncryptString(string plainText, string key)
        {
            // Initializes a 16-byte initialization vector (IV) with all zeros for AES encryption
            byte[] iv = new byte[16];
            byte[] array;

            // Creates and configures an AES encryption algorithm instance
            using (Aes aes = Aes.Create())
            {
                // Converts the provided key string to bytes using UTF-8 encoding
                aes.Key = Encoding.UTF8.GetBytes(key);
                // Sets the initialization vector for the encryption algorithm
                aes.IV = iv;
                // Creates an encryptor object using the key and IV
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                // Sets up memory stream to hold the encrypted data
                using MemoryStream memoryStream = new();
                // Creates a crypto stream that transforms data written to it using the encryptor
                using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
                // Uses a stream writer to write the plain text to the crypto stream
                using (StreamWriter streamWriter = new(cryptoStream))
                {
                    // Writes the plain text which gets encrypted through the crypto stream
                    streamWriter.Write(plainText);
                }
                // Retrieves the encrypted byte array from the memory stream
                array = memoryStream.ToArray();
            }
            // Converts the encrypted byte array to a Base64 string for safe storage/transmission
            return Convert.ToBase64String(array);
        }

        /// <summary>
        /// Decrypts the specified cipher text using the provided key.
        /// </summary>
        /// <param name="cipherText">The encrypted text in Base64 format.</param>
        /// <param name="key">The decryption key. Must match the key used for encryption.</param>
        /// <returns>The decrypted plain text string.</returns>
        public static string DecryptString(string cipherText, string key)
        {
            // Initializes a 16-byte initialization vector (IV) with all zeros (must match encryption IV)
            byte[] iv = new byte[16];
            // Converts the Base64 encoded cipher text back to a byte array
            byte[] buffer = Convert.FromBase64String(cipherText);

            // Creates and configures an AES decryption algorithm instance
            using Aes aes = Aes.Create();
            // Converts the provided key string to bytes using UTF-8 encoding (must match encryption key)
            aes.Key = Encoding.UTF8.GetBytes(key);
            // Sets the initialization vector to match the one used during encryption
            aes.IV = iv;
            // Creates a decryptor object using the key and IV
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            // Creates a memory stream from the encrypted byte array
            using MemoryStream memoryStream = new(buffer);
            // Creates a crypto stream that transforms data read from it using the decryptor
            using CryptoStream cryptoStream = new((Stream)memoryStream, decryptor, CryptoStreamMode.Read);
            // Uses a stream reader to read the decrypted text from the crypto stream
            using StreamReader streamReader = new((Stream)cryptoStream);
            // Reads the entire decrypted content and returns it as a string
            return streamReader.ReadToEnd();
        }
    }
}
