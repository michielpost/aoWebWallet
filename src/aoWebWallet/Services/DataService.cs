
using aoWebWallet.Models;
using ArweaveAO;
using ArweaveAO.Models.Token;

namespace aoWebWallet.Services
{
    public class DataService
    {
        private readonly StorageService storageService;
        private readonly TokenClient tokenClient;

        public DataService(StorageService storageService, TokenClient tokenClient)
        {
            this.storageService = storageService;
            this.tokenClient = tokenClient;
        }

        internal void Init(string value)
        {
           // throw new NotImplementedException();
        }

        public async Task<List<Token>> LoadTokenData()
        {
            var tokens = await storageService.GetTokenIds();
            foreach (var token in tokens)
            {
                if (token.TokenData == null)
                {
                    //Load metadata
                    try
                    {
                        var data = await tokenClient.GetTokenMetaData(token.TokenId);
                        token.TokenData = data;
                    }
                    catch { }
                }
            }

            await storageService.SaveTokenList(tokens);

            return tokens;
        }

        public async Task<List<BalanceData>> LoadWalletBalances(string address)
        {
            var tokens = await storageService.GetTokenIds();

            var result = new List<BalanceData>();

            foreach (var token in tokens)
            {
                var balanceData = await tokenClient.GetBalance(token.TokenId, address);
                if (balanceData != null)
                    result.Add(balanceData);
            }

            return result;
        }
    }
}
