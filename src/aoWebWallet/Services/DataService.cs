
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

        public async IAsyncEnumerable<Token> LoadTokenDataAsync()
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

                if (token.TokenData != null)
                    yield return token;
            }

            await storageService.SaveTokenList(tokens);

        }

    }
}
