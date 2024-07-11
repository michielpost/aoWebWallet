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

            var result = await graph.GetTokenTransfers("CeiYr2VjUVAFXmPJvfj-Pfk6zmprBzeqNeRWAbImbOo");

            Assert.IsNotNull(result);
        }
    }
}