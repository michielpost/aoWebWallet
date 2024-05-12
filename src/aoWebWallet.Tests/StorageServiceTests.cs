using aoWebWallet.Models;
using aoWebWallet.Services;
using ArweaveAO;

namespace aoWebWallet.Tests
{
    [TestClass]
    public class StorageServiceTests
    {
        [TestMethod]
        public async Task TestBuildInTokenData()
        {
            List<Token> result = new();

            StorageService.AddSystemTokens(result);

            TokenClient tokenClient = new TokenClient(new HttpClient());

           foreach(var token in result)
            {
                //Get live data
                var data = await tokenClient.GetTokenMetaData(token.TokenId);

                Assert.IsNotNull(token.TokenData);
                Assert.IsNotNull(data);

                Assert.AreEqual(token.TokenId, data.TokenId);
                Assert.AreEqual(token.TokenData.TokenId, data.TokenId);
                Assert.AreEqual(token.TokenData.Name, data.Name);
                Assert.AreEqual(token.TokenData.Ticker, data.Ticker);
                Assert.AreEqual(token.TokenData.Denomination, data.Denomination);
                Assert.AreEqual(token.TokenData.Logo, data.Logo);

            }
        }
    }
}