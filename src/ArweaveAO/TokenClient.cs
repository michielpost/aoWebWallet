using ArweaveAO.Models;
using ArweaveAO.Models.Token;
using ArweaveAO.Requests;

namespace ArweaveAO
{
    /// <summary>
    /// TokenClient, helper to interact with processes implementing the ao token standard
    /// </summary>
    public class TokenClient : AODataClient
    {
        public TokenClient(HttpClient http) : base("https://cu.ao-testnet.xyz", http) { }

        public async Task<TokenData?> GetTokenMetaData(string processId)
        {
            try
            {
                var request = new DryRunRequest
                { 
                    Target = processId,
                    Tags = new List<Tag>
                    {
                        new Tag { Name = "Action", Value = "Info"},
                        new Tag { Name = "Type", Value = "Message"},
                        new Tag { Name = "Variant", Value = "ao.TN.1"},
                        new Tag { Name = "Protocol", Value = "ao"},
                    }
                };
                var result = await DryRun(processId, request);
                if (result == null || !result.Messages.Any())
                    return null;

                var tokenData = new TokenData();
                tokenData.TokenId = processId;
                tokenData.Name = result.Messages.First().Tags.Where(x => x.Name == "Name").Select(x => x.Value).FirstOrDefault();
                tokenData.Ticker = result.Messages.First().Tags.Where(x => x.Name == "Ticker").Select(x => x.Value).FirstOrDefault();
                tokenData.Logo = result.Messages.First().Tags.Where(x => x.Name == "Logo").Select(x => x.Value).FirstOrDefault();
                
                
                string? denomination = result.Messages.First().Tags.Where(x => x.Name == "Denomination").Select(x => x.Value).FirstOrDefault();
                if(!string.IsNullOrWhiteSpace(denomination) && int.TryParse(denomination, out int denominationInt))
                    tokenData.Denomination = denominationInt;

                return tokenData;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("not found", StringComparison.InvariantCultureIgnoreCase))
                    return null;

                // Deal with exception
                throw;
            }
        }

        public async Task<BalanceData?> GetBalance(string tokenId, string address)
        {
            try
            {
                var request = new DryRunRequest
                {
                    Target = tokenId,
                    Tags = new List<Tag>
                    {
                        new Tag { Name = "Action", Value = "Balance"},
                        new Tag { Name = "Target", Value = address},
                        new Tag { Name = "Type", Value = "Message"},
                        new Tag { Name = "Variant", Value = "ao.TN.1"},
                        new Tag { Name = "Protocol", Value = "ao"},
                    }
                };
                var result = await DryRun(tokenId, request); 
                if (result == null || !result.Messages.Any())
                    return null;

                var balanceData = new BalanceData();
                balanceData.TokenId = tokenId;
                balanceData.Ticker = result.Messages.First().Tags.Where(x => x.Name == "Ticker").Select(x => x.Value).FirstOrDefault();
                balanceData.Account = result.Messages.First().Tags.Where(x => x.Name == "Account").Select(x => x.Value).FirstOrDefault();
                
                string? balance = result.Messages.First().Tags.Where(x => x.Name == "Balance").Select(x => x.Value).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(balance) && long.TryParse(balance, out long balanceLong))
                    balanceData.Balance = balanceLong;

                return balanceData;
            }
            catch (Exception e)
            {
                // Deal with exception
                throw;
            }
        }
    }
}
