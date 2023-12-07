using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Izm.Rumis.Api.Services
{
    public interface IEncryptionService
    {
        string Decrypt(string data);
        string Encrypt(string data);
    }

    public class EncryptionService : IEncryptionService
    {
        private readonly string password;
        private readonly string salt;

        public EncryptionService(string password, string salt)
        {
            this.password = password;
            this.salt = salt;
        }

        public string Encrypt(string data)
        {
            return WithAes(() =>
            {
                byte[] encrypted = EncryptStringToBytes(data);
                return Convert.ToBase64String(encrypted);
            });
        }

        public string Decrypt(string data)
        {
            return WithAes(() =>
            {
                // Encrypt the string to an array of bytes
                var decrypted = Convert.FromBase64String(data);
                return DecryptStringFromBytes(decrypted);
            });
        }

        private byte[] EncryptStringToBytes(string plainText)
        {
            byte[] encrypted = null;

            WithAes(aes =>
            {
                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            // Write all data to the stream
                            swEncrypt.Write(plainText);
                        }

                        encrypted = msEncrypt.ToArray();
                    }
                }
            });

            // Return the encrypted bytes from the memory stream
            return encrypted;
        }

        private string DecryptStringFromBytes(byte[] cipherText)
        {
            string plainText = null;

            WithAes(aes =>
            {
                // Create a decryptor to perform the stream transform
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                // Create the streams used for decryption
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string
                            plainText = srDecrypt.ReadToEnd();
                        }
                    }
                }
            });

            return plainText;
        }

        private void SetupAes(Aes aes)
        {
            byte[] saltBytes = Encoding.ASCII.GetBytes(salt);
            var key = new Rfc2898DeriveBytes(password, saltBytes);

            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);
        }

        private void WithAes(Action<Aes> fn)
        {
            using (var aes = Aes.Create("AesManaged"))
            {
                SetupAes(aes);
                fn(aes);
            }
        }

        private T WithAes<T>(Func<T> fn)
        {
            using (var aes = Aes.Create("AesManaged"))
            {
                SetupAes(aes);
                return fn();
            }
        }
    }
}
