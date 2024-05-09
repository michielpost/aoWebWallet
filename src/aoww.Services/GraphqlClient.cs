using aoww.Services.Models;
using System.Net.Http.Json;

namespace aoww.Services
{
    /// <summary>
    /// https://arweave.net/graphql
    /// </summary>
    public class GraphqlClient
    {
        private readonly HttpClient httpClient;

        public GraphqlClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<TokenTransfer>> GetTransactionsIn(string adddress, string? fromTxId = null)
        {
            string query = $$"""
                                query {
                                  transactions(
                                    first: 100
                                    sort: HEIGHT_DESC
                                    tags: [
                                      { name: "Data-Protocol", values: ["ao"] }
                                      { name: "Action", values: ["Transfer", "Mint-Token"] }
                                      { name: "Recipient", values: ["{{adddress}}"] }
                                    ]
                                  ) {
                                    edges {
                                      node {
                                        id
                                        recipient
                                        owner {
                                          address
                                        }
                                        block {
                                          timestamp
                                          height
                                        }
                                        tags {
                                          name
                                          value
                                        }
                                      }
                                    }
                                  }
                                }
                                
                                """;
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

        private static TokenTransfer? GetTransaction(Edge edge)
        {
            if (edge == null || edge.Node == null)
                return null;

            var isTransfer = edge.Node.Tags.Where(x => x.Name == "Action" && x.Value == "Transfer").Any();
            var isMint = edge.Node.Tags.Where(x => x.Name == "Action" && x.Value == "Mint-Token").Any();
            if (!isTransfer && !isMint)
                return null;

            var transaction = new TokenTransfer()
            {
                Id = edge.Node.Id,
                From = edge.Node.Owner?.Address ?? string.Empty,
                TokenTransferType = Enums.TokenTransferType.Transfer
            };

            if (isMint)
                transaction.TokenTransferType = Enums.TokenTransferType.Mint;


            if (edge.Node.Block != null)
            {
                transaction.Timestamp = DateTimeOffset.FromUnixTimeSeconds(edge.Node.Block.Timestamp);
                transaction.BlockHeight = edge.Node.Block.Height;
            }
            else
                transaction.Timestamp = DateTimeOffset.UtcNow;

            var fromProcess = edge.Node.Tags.Where(x => x.Name == "From-Process").Select(x => x.Value).FirstOrDefault();
            if (!string.IsNullOrEmpty(fromProcess))
                transaction.From = fromProcess;

            if (isMint)
                transaction.TokenId = edge.Node.Tags.Where(x => x.Name == "TokenId").Select(x => x.Value).FirstOrDefault();
            else
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
                Owner = edge.Node.Owner?.Address,
                Name = name,
            };

            processInfo.Version = edge.Node.Tags.Where(x => x.Name == "Version").Select(x => x.Value).FirstOrDefault();

            return processInfo;
        }

        public async Task<List<TokenTransfer>> GetTransactionsOut(string address, string? fromTxId = null)
        {
            string query = $$"""
                                query {
                                  transactions(
                                    first: 100
                                    sort: HEIGHT_DESC
                                    owners: ["{{address}}"]
                                    tags: [
                                      { name: "Data-Protocol", values: ["ao"] }
                                      { name: "Action", values: ["Transfer"] }
                                    ]
                                  ) {
                                    edges {
                                      node {
                                        id
                                        recipient
                                        owner {
                                          address
                                        }
                                        block {
                                          timestamp
                                          height
                                        }
                                        tags {
                                          name
                                          value
                                        }
                                      }
                                    }
                                  }
                                }
                                """;
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

        public async Task<List<TokenTransfer>> GetTransactionsOutFromProcess(string address, string? fromTxId = null)
        {
            string query = $$"""
                query {
                  transactions(
                    first: 100
                    sort: HEIGHT_DESC
                    tags: [
                      { name: "From-Process", values: ["{{address}}"] }
                      { name: "Data-Protocol", values: ["ao"] }
                      { name: "Action", values: ["Transfer"] }
                    ]
                  ) {
                    edges {
                      node {
                        id
                        recipient
                        owner {
                          address
                        }
                        block {
                          timestamp
                          height
                        }
                        tags {
                          name
                          value
                        }
                      }
                    }
                  }
                }
                
                """;
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
            string query = $$"""
                query {
                  transactions(
                    first: 1
                    sort: HEIGHT_DESC
                    ids: ["{{txId}}"]
                    tags: [
                      { name: "Data-Protocol", values: ["ao"] }
                      { name: "Action", values: ["Transfer"] }
                    ]
                  ) {
                    edges {
                      node {
                        id
                        recipient
                        owner {
                          address
                        }
                        block {
                          timestamp
                          height
                        }
                        tags {
                          name
                          value
                        }
                      }
                    }
                  }
                }                
                """;
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
            string query = $$"""
                query {
                  transactions(
                    first: 50
                    sort: HEIGHT_DESC
                    recipients: ["{{tokenId}}"]
                    tags: [
                      { name: "Data-Protocol", values: ["ao"] }
                      { name: "Action", values: ["Transfer"] }
                    ]
                  ) {
                    edges {
                      node {
                        id
                        recipient
                        owner {
                          address
                        }
                        block {
                          timestamp
                          height
                        }
                        tags {
                          name
                          value
                        }
                      }
                    }
                  }
                }                
                """;
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
            string query = $$"""
                query {
                  transactions(
                    first: 100
                    owners: ["{{address}}"]
                    tags: [
                      { name: "Data-Protocol", values: ["ao"] }
                      { name: "Type", values: ["Process"] }
                      { name: "App-Name", values: ["aos"] }
                    ]
                  ) {
                    edges {
                      node {
                        id
                        tags {
                          name
                          value
                        }
                      }
                    }
                  }
                }
                """;
            var queryResult = await PostQueryAsync(query);

            var result = new List<AoProcessInfo>();

            Console.WriteLine("AOresult:" + (queryResult?.Data?.Transactions?.Edges.Count ?? 0).ToString());

            foreach (var edge in queryResult?.Data?.Transactions?.Edges ?? new())
            {
                AoProcessInfo? processInfo = GetAoProcessInfo(edge);

                if (processInfo != null)
                    result.Add(processInfo);
            }

            return result;
        }

        public async Task<AoProcessInfo?> GetOwnerForAoProcessAddress(string address)
        {
            string query = $$"""
                query {
                  transactions(
                    first: 50
                    ids: ["{{address}}"]
                    tags: [
                      { name: "Data-Protocol", values: ["ao"] }
                      { name: "Type", values: ["Process"] }
                      { name: "App-Name", values: ["aos"] }
                    ]
                  ) {
                    edges {
                      node {
                        id
                        owner {
                          address
                        }
                        tags {
                          name
                          value
                        }
                      }
                    }
                  }
                }                
                """;
            var queryResult = await PostQueryAsync(query);

            var result = new List<AoProcessInfo>();

            foreach (var edge in queryResult?.Data?.Transactions?.Edges ?? new())
            {
                AoProcessInfo? processInfo = GetAoProcessInfo(edge);

                if (processInfo != null)
                    result.Add(processInfo);
            }

            return result.FirstOrDefault();
        }

        protected async Task<GraphqlResponse?> PostQueryAsync(string query)
        {
            var request = new GraphqlRequest { Query = query };

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
