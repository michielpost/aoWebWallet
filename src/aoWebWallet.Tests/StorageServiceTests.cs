using aoWebWallet.Models;
using aoWebWallet.Services;
using ArweaveAO;
using ArweaveAO.Models;
using Microsoft.Extensions.Options;

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

            TokenClient tokenClient = new TokenClient(Options.Create(new ArweaveConfig()), new HttpClient());

            result.Reverse();

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