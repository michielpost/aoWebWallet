using aoww.Services;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace aoww.Services.Tests
{
    [TestClass]
    public class EncryptionTests
    {

        [TestMethod]
        public void BasicEncryptionTest()
        {
            var key = EncryptionService.GenerateKeys("test");
            var key2 = EncryptionService.GenerateKeys("bar");

            var testValue = "this is not encrypted";

            var encrypted = EncryptionService.Encrypt(System.Text.Encoding.UTF8.GetBytes(testValue), key.privateKey);
            var eString = System.Text.Encoding.UTF8.GetString(encrypted);
            Assert.AreNotEqual(testValue, eString);


            var decrypt = EncryptionService.Decrypt(encrypted, key.privateKey);
            var dString = System.Text.Encoding.UTF8.GetString(decrypt);

            Assert.AreEqual(testValue, dString);
        }

        [TestMethod]
        public void WalletTest()
        {
            var key = "test";
            var jwk = "1234";

            var encrypted = EncryptionService.EncryptWallet(key, jwk);
            var decrypt = EncryptionService.DecryptWallet(key, encrypted);

            Assert.AreEqual(jwk, decrypt);
        }

        [TestMethod]
        public void EncryptWallet_SamePassword_ProducesDifferentCiphertextAndNonce()
        {
            // Regression test: EncryptWallet used to derive the nonce from the password
            // alone, so encrypting two different wallets with the same password produced
            // identical ciphertext prefixes (nonce reuse), which lets an attacker recover
            // both plaintexts via a two-time-pad attack. Each call must now use a fresh
            // random salt/nonce even when the password is identical.
            var password = "same-password";

            var encryptedOne = EncryptionService.EncryptWallet(password, "jwk-one");
            var encryptedTwo = EncryptionService.EncryptWallet(password, "jwk-one");

            Assert.AreNotEqual(encryptedOne, encryptedTwo);

            Assert.AreEqual("jwk-one", EncryptionService.DecryptWallet(password, encryptedOne));
            Assert.AreEqual("jwk-one", EncryptionService.DecryptWallet(password, encryptedTwo));
        }

        [TestMethod]
        public void DecryptWallet_LegacyFormat_StillDecrypts()
        {
            // Wallets encrypted before the nonce-reuse fix don't have the "v2:" prefix.
            // They must remain decryptable so existing users don't lose access to wallets
            // already stored in their browser.
            var password = "legacy-password";
            var jwk = "legacy-jwk-contents";

            var key = EncryptionService.GenerateKeys(password);
            var legacyEncrypted = EncryptionService.Encrypt(System.Text.Encoding.UTF8.GetBytes(jwk), key.privateKey);
            var legacyEncryptedHex = BitConverter.ToString(legacyEncrypted).Replace("-", "");

            var decrypted = EncryptionService.DecryptWallet(password, legacyEncryptedHex);

            Assert.AreEqual(jwk, decrypted);
        }

        [TestMethod]
        public void DecryptWallet_WrongPassword_ThrowsCryptographicException()
        {
            // Previously a wrong password made the underlying library return null, which
            // crashed with an unhandled ArgumentNullException instead of a clear error.
            var encrypted = EncryptionService.EncryptWallet("correct-password", "jwk");

            try
            {
                EncryptionService.DecryptWallet("wrong-password", encrypted);
                Assert.Fail("Expected a CryptographicException for a wrong password.");
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                // Expected.
            }
        }
    }
}
