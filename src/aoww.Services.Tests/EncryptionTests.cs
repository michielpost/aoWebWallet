using aoww.Services;
using SiaSkynet;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SiaSkynet.Tests
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


    }
}
