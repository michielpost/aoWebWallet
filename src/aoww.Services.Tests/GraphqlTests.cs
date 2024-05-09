namespace aoww.Services.Tests
{
    [TestClass]
    public class GraphqlTests
    {
        [TestMethod]
        public async Task GetTransactionsTest()
        {
            var graph = new GraphqlClient(new HttpClient());

            var result = await graph.GetTransactionsIn("4NdFkWsgFQIEmJnzFSYrO88UmRPf0ABfVh_fRc2u130");

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetMintTest()
        {
            var graph = new GraphqlClient(new HttpClient());

            var result = await graph.GetTransactionsIn("CeiYr2VjUVAFXmPJvfj-Pfk6zmprBzeqNeRWAbImbOo");

            Assert.IsNotNull(result);
        }
    }
}