using aoWebWallet.Models;
using ArweaveAO.Models.Token;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace aoWebWallet.Services
{
    public class GraphqlClient
    {
        private readonly HttpClient httpClient;

        public GraphqlClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<TokenTransfer>> GetTransactionsIn(string adddress, string? fromTxId = null)
        {
            string query = "query {\r\n  transactions(\r\n    first: 100\r\n sort: HEIGHT_DESC\r\n    tags: [\r\n      { name: \"Data-Protocol\", values: [\"ao\"] }\r\n      { name: \"Action\", values: [\"Transfer\"] }\r\n      { name: \"Recipient\", values: [\"" + adddress + "\"] }\r\n    ]\r\n  ) {\r\n    edges {\r\n      node {\r\n        id\r\n        recipient\r\n        owner {\r\n          address\r\n        }\r\n        block {\r\n          timestamp\r\n          height\r\n        }\r\n        tags {\r\n          name\r\n          value\r\n        }\r\n      }\r\n    }\r\n  }\r\n}\r\n";
            var queryResult = await PostQueryAsync(query);

            var result = new List<TokenTransfer>();

            foreach (var edge in queryResult?.Data?.Transactions?.Edges ?? new())
            {
                TokenTransfer? transaction = GetTransaction(edge);

                if(transaction != null)
                    result.Add(transaction);
            }

            return result;
        }

        private static TokenTransfer? GetTransaction(Edge edge)
        {
            if (edge == null || edge.Node == null)
                return null;

            var isTransfer = edge.Node.Tags.Where(x => x.Name == "Action" && x.Value == "Transfer").Any();
            if (!isTransfer)
                return null;

            var transaction = new TokenTransfer()
            {
                Id = edge.Node.Id,
                From = edge.Node.Owner?.Address ?? string.Empty
            };

            if (edge.Node.Block != null)
            {
                transaction.Timestamp = DateTimeOffset.FromUnixTimeSeconds(edge.Node.Block.Timestamp);
                transaction.BlockHeight = edge.Node.Block.Height;
            }
            else
                transaction.Timestamp = DateTimeOffset.UtcNow;

            transaction.TokenId = edge.Node.Recipient;
            transaction.To = edge.Node.Tags.Where(x => x.Name == "Recipient").Select(x => x.Value).FirstOrDefault();

            string? quantity = edge.Node.Tags.Where(x => x.Name == "Quantity").Select(x => x.Value).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(quantity) && long.TryParse(quantity, out long quantityLong))
                transaction.Quantity = quantityLong;
            return transaction;
        }

        private static AoProcessInfo? GetAoProcessInfo(Edge edge)
        {
            if (edge == null || edge.Node == null)
                return null;


            var name = edge.Node.Tags.Where(x => x.Name == "Name").Select(x => x.Value).FirstOrDefault();
            if (string.IsNullOrEmpty(name))
                return null;

            var processInfo = new AoProcessInfo()
            {
                Id = edge.Node.Id,
                Name = name,
            };

            processInfo.Version = edge.Node.Tags.Where(x => x.Name == "Version").Select(x => x.Value).FirstOrDefault();

            return processInfo;
        }

        public async Task<List<TokenTransfer>> GetTransactionsOut(string adddress, string? fromTxId = null)
        {
            string query = "query {\r\n  transactions(\r\n    first: 100\r\n    sort: HEIGHT_DESC\r\n owners: [\"" + adddress + "\"]\r\n    tags: [\r\n      { name: \"Data-Protocol\", values: [\"ao\"] }\r\n      { name: \"Action\", values: [\"Transfer\"] }\r\n    ]\r\n  ) {\r\n    edges {\r\n      node {\r\n        id\r\n        recipient\r\n        owner {\r\n          address\r\n        }\r\n        block {\r\n          timestamp\r\n          height\r\n        }\r\n        tags {\r\n          name\r\n          value\r\n        }\r\n      }\r\n    }\r\n  }\r\n}\r\n";
            var queryResult = await PostQueryAsync(query);

            var result = new List<TokenTransfer>();

            foreach (var edge in queryResult?.Data?.Transactions?.Edges ?? new())
            {
                TokenTransfer? transaction = GetTransaction(edge);

                if (transaction != null)
                    result.Add(transaction);
            }

            return result;
        }

        public async Task<TokenTransfer?> GetTransactionsById(string txId)
        {
            string query = "query {\r\n  transactions(\r\n    first: 1\r\n sort: HEIGHT_DESC\r\n   ids: [\"" + txId + "\"]\r\n    tags: [\r\n      { name: \"Data-Protocol\", values: [\"ao\"] }\r\n      { name: \"Action\", values: [\"Transfer\"] }\r\n    ]\r\n  ) {\r\n    edges {\r\n      node {\r\n        id\r\n        recipient\r\n        owner {\r\n          address\r\n        }\r\n        block {\r\n          timestamp\r\n          height\r\n        }\r\n        tags {\r\n          name\r\n          value\r\n        }\r\n      }\r\n    }\r\n  }\r\n}\r\n";
            var queryResult = await PostQueryAsync(query);

            var result = new List<TokenTransfer>();

            foreach (var edge in queryResult?.Data?.Transactions?.Edges ?? new())
            {
                TokenTransfer? transaction = GetTransaction(edge);

                if (transaction != null)
                    result.Add(transaction);
            }

            return result.FirstOrDefault();
        }

        public async Task<List<TokenTransfer>> GetTransactionsForToken(string tokenId, string? fromTxId = null)
        {
            string query = "query {\r\n  transactions(\r\n    first: 50\r\n sort: HEIGHT_DESC\r\n   recipients: [\"" + tokenId + "\"]\r\n    tags: [\r\n      { name: \"Data-Protocol\", values: [\"ao\"] }\r\n      { name: \"Action\", values: [\"Transfer\"] }\r\n    ]\r\n  ) {\r\n    edges {\r\n      node {\r\n        id\r\n        recipient\r\n        owner {\r\n          address\r\n        }\r\n        block {\r\n          timestamp\r\n          height\r\n        }\r\n        tags {\r\n          name\r\n          value\r\n        }\r\n      }\r\n    }\r\n  }\r\n}\r\n";
            var queryResult = await PostQueryAsync(query);

            var result = new List<TokenTransfer>();

            foreach (var edge in queryResult?.Data?.Transactions?.Edges ?? new())
            {
                TokenTransfer? transaction = GetTransaction(edge);

                if (transaction != null)
                    result.Add(transaction);
            }

            return result;
        }

        public async Task<List<AoProcessInfo>> GetAoProcessesForAddress(string address)
        {
            string query = "query {\r\n    transactions(\r\n      first: 100,\r\n      owners: [\"" + address + "\"],\r\n      tags: [\r\n        { name: \"Data-Protocol\", values: [\"ao\"] },\r\n        { name: \"Type\", values: [\"Process\"]},\r\n        { name: \"App-Name\", values: [\"aos\"]},\r\n        \r\n      ]\r\n    ) {\r\n      edges {\r\n        node {\r\n          id\r\n          tags {\r\n          name\r\n          value\r\n        }\r\n        }\r\n      }\r\n    }\r\n  }";
            var queryResult = await PostQueryAsync(query);

            var result = new List<AoProcessInfo>();

            foreach (var edge in queryResult?.Data?.Transactions?.Edges ?? new())
            {
                AoProcessInfo? processInfo = GetAoProcessInfo(edge);

                if (processInfo != null)
                    result.Add(processInfo);
            }

            return result;
        }

        //public async Task GetTransactionsForToken(string tokenId, string fromTxId)
        //{

        //}

        protected async Task<GraphqlResponse?> PostQueryAsync(string query)
        {
            var request = new GraphqlRequest { Query = query};

            HttpResponseMessage res = await httpClient.PostAsJsonAsync("https://arweave.net/graphql", request);
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadFromJsonAsync<GraphqlResponse>();
            }
            else
            {
                string msg = await res.Content.ReadAsStringAsync();
                Console.WriteLine(msg);
                throw new Exception(msg);
            }
        }
    }
}
