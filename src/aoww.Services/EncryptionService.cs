using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace aoww.Services
{
    public static class EncryptionService
    {

        /// <summary>
        /// Generates a predicatable key pair based on the seed
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static (byte[] privateKey, byte[] publicKey) GenerateKeys(string seed)
        {
            var masterKey = GetMasterKeyFromSeed(seed);
            Chaos.NaCl.Ed25519.KeyPairFromSeed(out byte[] publicKey, out byte[] privateKey, masterKey);

            if (publicKey == null)
                throw new Exception("Failed to generate public key");

            return (privateKey, publicKey);
        }

        /// <summary>
        /// Generate a private key
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        private static byte[] GetMasterKeyFromSeed(string seed)
        {
            SHA256 hasher = SHA256.Create();
            byte[] keyBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(seed));
            var i = keyBytes.AsSpan();
            return i.Slice(0, 32).ToArray();
        }

        public static byte[] Encrypt(byte[] data, byte[] privateKey)
        {
            byte[] nonce = privateKey[0..24];
            byte[] shortKey = privateKey[0..32];

            return Chaos.NaCl.XSalsa20Poly1305.Encrypt(data, shortKey, nonce);
        }

        public static byte[] Decrypt(byte[] ciphertext, byte[] privateKey)
        {
            byte[] nonce = privateKey[0..24];
            byte[] shortKey = privateKey[0..32];

            return Chaos.NaCl.XSalsa20Poly1305.TryDecrypt(ciphertext, shortKey, nonce);
        }
    }
}
