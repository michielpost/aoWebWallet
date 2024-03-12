using ArweaveAO.Models;
using ArweaveAO.Requests;
using ArweaveAO.Responses;
using System.Diagnostics;

namespace ArweaveAO
{
    public class AODataClient : ClientAPI
    {
        public AODataClient(HttpClient http) : base("https://cu.ao-testnet.xyz", http) { }
        public AODataClient(string baseRoute, HttpClient http) : base(baseRoute, http) { }

        public async Task<MessageResult?> DryRun(string processId, DryRunRequest request)
        {
            try
            {
                var result = await PostAsync<MessageResult?, DryRunRequest>($"dry-run?process-id={processId}", request);
                return result;

            }
            catch (Exception e)
            {
                // Deal with exception
                throw;
            }
        }

        public async Task<MessageResult?> GetResult(string processId, string msgId)
        {
            try
            {
                var result = await GetAsync<MessageResult?>($"result/{msgId}?process-id={processId}");
                return result;

            }
            catch (Exception e)
            {
                // Deal with exception
                throw;
            }
        }

    }
}
