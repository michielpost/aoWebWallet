using aoww.Services.Extensions;
using aoww.Services.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace aoww.Services
{
    /// <summary>
    /// https://arweave.net/graphql
    /// </summary>
    public class GraphqlClient
    {
        private readonly HttpClient httpClient;
        private readonly GraphqlConfig config;

        public GraphqlClient(HttpClient httpClient, IOptions<GraphqlConfig> config)
        {
            this.httpClient = httpClient;
            this.config = config.Value;
        }

        public async Task<List<AoTransaction>> GetTransactionsIn(string address, string? cursor = null)
        {
            string query = $$"""
                                query {
                                  transactions(
                                    first: 50
                                    after: "{{cursor}}"
                                    sort: HEIGHT_DESC
                                    tags: [
                                      { name: "Data-Protocol", values: ["ao"] }
                                      { name: "Recipient", values: ["{{address}}"] }
                                    ]
                                  ) {
                                    edges {
                                      cursor
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

            var result = new List<AoTransaction>();

            foreach (var edge in queryResult?.Data?.Transactions?.Edges ?? new())
            {
                AoTransaction? transaction = GetAoTransaction(edge);

                if (transaction != null)
                    result.Add(transaction);
            }

            return result;
        }


        public async Task<List<TokenTransfer>> GetTokenTransfers(string address, string? cursor = null)
        {
            string query = $$"""
                                query {
                                  transactions(
                                    first: 50
                                    after: "{{cursor}}"
                                    sort: HEIGHT_DESC
                                    recipients: ["{{address}}"]
                                    tags: [
                                      { name: "Data-Protocol", values: ["ao"] }
                                      { name: "Action", values: ["Credit-Notice", "Debit-Notice", "Mint", "Mint-Token"] }
                                    ]
                                  ) {
                                    edges {
                                      cursor
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
                TokenTransfer? transaction = GetTokenTransfer(edge);

                if (transaction != null)
                    result.Add(transaction);
            }

            return result;
        }

        private static AoTransaction? GetAoTransaction(Edge edge)
        {
            if (edge == null || edge.Node == null)
                return null;

            var transaction = new AoTransaction()
            {
                Id = edge.Node.Id,
                Cursor = edge.Cursor,
                From = edge.Node.Owner?.Address ?? string.Empty,
            };

            if (edge.Node.Block != null)
            {
                transaction.Timestamp = DateTimeOffset.FromUnixTimeSeconds(edge.Node.Block.Timestamp);
                transaction.BlockHeight = edge.Node.Block.Height;
            }
            else
                transaction.Timestamp = DateTimeOffset.UtcNow;

            var fromProcess = edge.GetFirstTagValue("From-Process");
            if (!string.IsNullOrEmpty(fromProcess))
                transaction.From = fromProcess;

           
            transaction.To = edge.GetFirstTagValue("Recipient");

            return transaction;
        }

        private static TokenTransfer? GetTokenTransfer(Edge edge)
        {
            if (edge == null || edge.Node == null)
                return null;

            var action = edge.GetFirstTagValue("Action");

            if (action == "Transfer")
                return GetTransferAction(edge);
            if (action == "Mint")
                return GetMintAction(edge);
            if (action == "Credit-Notice")
                return GetCreditNoticeAction(edge);
            if (action == "Debit-Notice")
                return GetDebitNoticeAction(edge);

            return null;
        }

        private static TokenTransfer? GetCreditNoticeAction(Edge edge)
        {
            if (edge == null || edge.Node == null)
                return null;

            var transaction = new TokenTransfer()
            {
                Id = edge.Node.Id,
                Cursor = edge.Cursor,
                TokenId = edge.GetFirstTagValue("From-Process") ?? string.Empty,
                From = edge.GetFirstTagValue("Sender") ?? string.Empty,
                To = edge.Node.Recipient ?? string.Empty,
                TokenTransferType = Enums.TokenTransferType.Transfer
            };

            if (edge.Node.Block != null)
            {
                transaction.Timestamp = DateTimeOffset.FromUnixTimeSeconds(edge.Node.Block.Timestamp);
                transaction.BlockHeight = edge.Node.Block.Height;
            }
            else
                transaction.Timestamp = DateTimeOffset.UtcNow;

            string? quantity = edge.GetFirstTagValue("Quantity");
            if (!string.IsNullOrWhiteSpace(quantity) && long.TryParse(quantity, out long quantityLong))
                transaction.Quantity = quantityLong;

            return transaction;
        }

        private static TokenTransfer? GetDebitNoticeAction(Edge edge)
        {
            if (edge == null || edge.Node == null)
                return null;

            var transaction = new TokenTransfer()
            {
                Id = edge.Node.Id,
                Cursor = edge.Cursor,
                TokenId = edge.GetFirstTagValue("From-Process") ?? string.Empty,
                From = edge.Node.Recipient ?? string.Empty,
                To = edge.GetFirstTagValue("Recipient") ?? string.Empty,
                TokenTransferType = Enums.TokenTransferType.Transfer
            };

            if (edge.Node.Block != null)
            {
                transaction.Timestamp = DateTimeOffset.FromUnixTimeSeconds(edge.Node.Block.Timestamp);
                transaction.BlockHeight = edge.Node.Block.Height;
            }
            else
                transaction.Timestamp = DateTimeOffset.UtcNow;

            string? quantity = edge.GetFirstTagValue("Quantity");
            if (!string.IsNullOrWhiteSpace(quantity) && long.TryParse(quantity, out long quantityLong))
                transaction.Quantity = quantityLong;

            return transaction;
        }

        private static TokenTransfer? GetMintAction(Edge edge)
        {
            if (edge == null || edge.Node == null)
                return null;

            var transaction = new TokenTransfer()
            {
                Id = edge.Node.Id,
                Cursor = edge.Cursor,
                From = edge.Node.Owner?.Address ?? string.Empty,
                TokenTransferType = Enums.TokenTransferType.Mint
            };


            if (edge.Node.Block != null)
            {
                transaction.Timestamp = DateTimeOffset.FromUnixTimeSeconds(edge.Node.Block.Timestamp);
                transaction.BlockHeight = edge.Node.Block.Height;
            }
            else
                transaction.Timestamp = DateTimeOffset.UtcNow;

            var fromProcess = edge.GetFirstTagValue("From-Process");
            if (!string.IsNullOrEmpty(fromProcess))
                transaction.From = fromProcess;

            transaction.TokenId = edge.GetFirstTagValue("TokenId");

            transaction.To = edge.GetFirstTagValue("Recipient");

            string? quantity = edge.GetFirstTagValue("Quantity");
            if (!string.IsNullOrWhiteSpace(quantity) && long.TryParse(quantity, out long quantityLong))
                transaction.Quantity = quantityLong;
            return transaction;
        }

        private static TokenTransfer? GetTransferAction(Edge edge)
        {
            if (edge == null || edge.Node == null)
                return null;

            var transaction = new TokenTransfer()
            {
                Id = edge.Node.Id,
                Cursor = edge.Cursor,
                From = edge.Node.Owner?.Address ?? string.Empty,
                TokenTransferType = Enums.TokenTransferType.Transfer
            };


            if (edge.Node.Block != null)
            {
                transaction.Timestamp = DateTimeOffset.FromUnixTimeSeconds(edge.Node.Block.Timestamp);
                transaction.BlockHeight = edge.Node.Block.Height;
            }
            else
                transaction.Timestamp = DateTimeOffset.UtcNow;

            var fromProcess = edge.GetFirstTagValue("From-Process");
            if (!string.IsNullOrEmpty(fromProcess))
                transaction.From = fromProcess;

            transaction.TokenId = edge.Node.Recipient;

            transaction.To = edge.GetFirstTagValue("Recipient");

            string? quantity = edge.GetFirstTagValue("Quantity");
            if (!string.IsNullOrWhiteSpace(quantity) && long.TryParse(quantity, out long quantityLong))
                transaction.Quantity = quantityLong;
            return transaction;
        }

        private static AoProcessInfo? GetAoProcessInfo(Edge edge)
        {
            if (edge == null || edge.Node == null)
                return null;


            var name = edge.GetFirstTagValue("Name");
            if (string.IsNullOrEmpty(name))
                return null;

            var processInfo = new AoProcessInfo()
            {
                Id = edge.Node.Id,
                Owner = edge.Node.Owner?.Address,
                Name = name,
            };

            processInfo.Version = edge.GetFirstTagValue("Version");

            return processInfo;
        }

        public async Task<List<TokenTransfer>> GetTransactionsOutFromProcess(string address, string? cursor = null)
        {
            string query = $$"""
                query {
                  transactions(
                    first: 50
                    after: "{{cursor}}"
                    sort: HEIGHT_DESC
                    tags: [
                      { name: "From-Process", values: ["{{address}}"] }
                      { name: "Data-Protocol", values: ["ao"] }
                      { name: "Action", values: ["Transfer"] }
                    ]
                  ) {
                    edges {
                      cursor
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
                TokenTransfer? transaction = GetTokenTransfer(edge);

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
                TokenTransfer? transaction = GetTokenTransfer(edge);

                if (transaction != null)
                    result.Add(transaction);
            }

            return result.FirstOrDefault();
        }

        public async Task<List<TokenTransfer>> GetTransactionsForToken(string tokenId, string? cursor = null)
        {
            string query = $$"""
                query {
                  transactions(
                    first: 50
                    after: "{{cursor}}"
                    sort: HEIGHT_DESC
                    recipients: ["{{tokenId}}"]
                    tags: [
                      { name: "Data-Protocol", values: ["ao"] }
                      { name: "Action", values: ["Transfer"] }
                    ]
                  ) {
                    edges {
                      cursor
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
                TokenTransfer? transaction = GetTokenTransfer(edge);

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

            HttpResponseMessage res = await httpClient.PostAsJsonAsync(config.ApiUrl, request);
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
