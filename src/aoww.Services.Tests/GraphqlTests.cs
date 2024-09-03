using aoww.Services.Models;
using Microsoft.Extensions.Options;

namespace aoww.Services.Tests
{
    [TestClass]
    public class GraphqlTests
    {
        [TestMethod]
        public async Task GetTransactionsTest()
        {
            var graph = new GraphqlClient(new HttpClient(), Options.Create<GraphqlConfig>(new()));

            var result = await graph.GetTokenTransfers("aGeRSnWykicBEGESPbTXPQ0_q2IiMLBBMyemu2pBYoA");

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetMintTest()
        {
            var graph = new GraphqlClient(new HttpClient(), Options.Create<GraphqlConfig>(new()));

            //var result = await graph.GetTokenTransfers("CeiYr2VjUVAFXmPJvfj-Pfk6zmprBzeqNeRWAbImbOo");
            var result = await graph.GetTokenTransfersIn("YCH7ugqKwIo_bQebG_eT_7QY0sxVXbnv7BTgmRi7FZk");

            Assert.IsNotNull(result);
        }
    }
}