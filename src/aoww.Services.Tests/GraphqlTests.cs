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

            var result = await graph.GetTokenTransfersIn("4NdFkWsgFQIEmJnzFSYrO88UmRPf0ABfVh_fRc2u130");

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetMintTest()
        {
            var graph = new GraphqlClient(new HttpClient(), Options.Create<GraphqlConfig>(new()));

            var result = await graph.GetTokenTransfersIn("CeiYr2VjUVAFXmPJvfj-Pfk6zmprBzeqNeRWAbImbOo");

            Assert.IsNotNull(result);
        }
    }
}