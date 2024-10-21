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


    }
}
