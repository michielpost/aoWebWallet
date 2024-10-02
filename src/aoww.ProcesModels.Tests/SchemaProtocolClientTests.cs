using aoww.ProcesModels.SchemaProtocol;
using ArweaveAO;
using ArweaveAO.Models;
using Microsoft.Extensions.Options;

namespace aoww.ProcesModels.Tests
{
    [TestClass]
    public sealed class SchemaProtocolClientTests
    {
        [TestMethod]
        public async Task GetActionsTests()
        {
            var processId = "ptvbacSmqJPfgCXxPc9bcobs5Th2B_SxTf81vRNkRzk";

            var client = new SchemaProtocolClient(new AODataClient(Options.Create<ArweaveConfig>(new()), new HttpClient()));

            var result = await client.GetSchemaProtocolActions(processId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task GetExternalActionsTests()
        {
            var processId = "kPjfXLFyjJogxGRRRe2ErdYNiexolpHpK6wGkz-UPVA";

            var client = new SchemaProtocolClient(new AODataClient(Options.Create<ArweaveConfig>(new()), new HttpClient()));

            var result = await client.GetSchemaProtocolActions(processId, schemaExternal: true);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task NoActionsTests()
        {
            var processId = "YCH7ugqKwIo_bQebG_eT_7QY0sxVXbnv7BTgmRi7FZk";

            var client = new SchemaProtocolClient(new AODataClient(Options.Create<ArweaveConfig>(new()), new HttpClient()));

            var result = await client.GetSchemaProtocolActions(processId);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }
    }
}
