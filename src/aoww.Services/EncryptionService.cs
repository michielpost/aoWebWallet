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
        // Marks the new encryption format: a random salt + random nonce per encryption,
        // with a PBKDF2-derived key. Kept distinguishable from the legacy hex-only output
        // (which never contains ':') so old wallets already stored in the browser keep
        // decrypting while every new encryption is done safely.
        private const string V2Prefix = "v2:";
        private const int SaltSizeBytes = 16;
        private const int NonceSizeBytes = 24;
        private const int KeySizeBytes = 32;
        private const int Pbkdf2Iterations = 210_000;

        /// <summary>
        /// Encrypts a wallet JWK with a password.
        /// Uses a random salt and nonce per call, so encrypting multiple wallets with the
        /// same password never reuses the same key/nonce pair.
        /// </summary>
        public static string EncryptWallet(string secretKey, string jwk)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
            var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);

            var key = DeriveKeyV2(secretKey, salt);
            var ciphertext = Chaos.NaCl.XSalsa20Poly1305.Encrypt(System.Text.Encoding.UTF8.GetBytes(jwk), key, nonce);

            var payload = new byte[salt.Length + nonce.Length + ciphertext.Length];
            Buffer.BlockCopy(salt, 0, payload, 0, salt.Length);
            Buffer.BlockCopy(nonce, 0, payload, salt.Length, nonce.Length);
            Buffer.BlockCopy(ciphertext, 0, payload, salt.Length + nonce.Length, ciphertext.Length);

            return V2Prefix + BitConverter.ToString(payload).Replace("-", "");
        }

        public static string DecryptWallet(string secretKey, string encryptedJwk)
        {
            if (encryptedJwk.StartsWith(V2Prefix, StringComparison.Ordinal))
            {
                var payload = HexStringToByteArray(encryptedJwk.Substring(V2Prefix.Length));

                var salt = payload[0..SaltSizeBytes];
                var nonce = payload[SaltSizeBytes..(SaltSizeBytes + NonceSizeBytes)];
                var ciphertext = payload[(SaltSizeBytes + NonceSizeBytes)..];

                var key = DeriveKeyV2(secretKey, salt);
                var decrypted = Chaos.NaCl.XSalsa20Poly1305.TryDecrypt(ciphertext, key, nonce);
                if (decrypted == null)
                    throw new CryptographicException("Failed to decrypt wallet. Wrong password or corrupted data.");

                return System.Text.Encoding.UTF8.GetString(decrypted);
            }

            // Legacy format: the nonce was derived from the password-derived key itself,
            // so the same password always produced the same nonce/key pair. That is unsafe
            // (it lets an attacker who obtains two wallets encrypted with the same password
            // recover both JWKs via a two-time-pad attack) but is kept here, read-only, so
            // wallets encrypted before this fix can still be unlocked.
            var legacyKey = EncryptionService.GenerateKeys(secretKey);
            var cipherData = HexStringToByteArray(encryptedJwk);

            var decrypt = EncryptionService.Decrypt(cipherData, legacyKey.privateKey);
            if (decrypt == null)
                throw new CryptographicException("Failed to decrypt wallet. Wrong password or corrupted data.");

            return System.Text.Encoding.UTF8.GetString(decrypt);
        }

        private static byte[] DeriveKeyV2(string secretKey, byte[] salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(System.Text.Encoding.UTF8.GetBytes(secretKey), salt, Pbkdf2Iterations, HashAlgorithmName.SHA256, KeySizeBytes);
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

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
