
using aoWebWallet.Models;
using aoWebWallet.Pages;
using ArweaveAO;
using ArweaveAO.Models.Token;
using System.Collections.ObjectModel;
using webvNext.DataLoader;

namespace aoWebWallet.Services
{
    public class TokenDataService
    {
        public DataLoader TokenDataLoader { get; set; } = new();
        public ObservableCollection<Token> TokenList { get; } = new();


        private readonly StorageService storageService;
        private readonly TokenClient tokenClient;

        public TokenDataService(StorageService storageService, TokenClient tokenClient)
        {
            this.storageService = storageService;
            this.tokenClient = tokenClient;
        }

        public async Task TryAddTokenIds(List<string?> allTokenIds)
        {
            foreach (var tokenId in allTokenIds)
            {
                if (string.IsNullOrEmpty(tokenId) || tokenId.Length != 43)
                    continue;

                var exist = TokenList.Where(x => x.TokenId == tokenId).Any();
                if (exist)
                    continue;

                var data = await TokenDataLoader.LoadAsync(async () =>
                {
                    var data = await tokenClient.GetTokenMetaData(tokenId);
                    return data;
                });

                if (data != null)
                {
                    await storageService.AddToken(tokenId, data, isUserAdded: false);
                }
            }

            await LoadTokenList();
        }

        public async Task<Token> LoadTokenAsync(string tokenId)
        {
            var tokens = await storageService.GetTokenIds();

            var token = tokens.Where(x => x.TokenId.Equals(tokenId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if(token == null)
            {
                token = new Token()
                { 
                    TokenId  = tokenId,
                };

                tokens.Add(token);
            }

            if (token.TokenData == null)
            {
                var data = await TokenDataLoader.LoadAsync(async () =>
                {
                    var data = await tokenClient.GetTokenMetaData(tokenId);
                    return data;
                });

                if (data != null)
                {

                    token.TokenData = data;

                    await storageService.SaveTokenList(tokens);
                }
               
            }

            return token;
        }

        public async Task LoadTokenList(bool force = false)
        {
            if (!TokenList.Any() || force)
            {
                TokenList.Clear();
                await foreach (var item in LoadTokenDataAsync())
                {
                    TokenList.Add(item);
                }
            }
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
                        var data = await TokenDataLoader.LoadAsync(async () =>
                        {
                            var data = await tokenClient.GetTokenMetaData(token.TokenId);
                            return data;
                        });

                        token.TokenData = data;
                    }
                    catch { }
                }

                if (token.TokenData != null)
                    yield return token;
            }

            await storageService.SaveTokenList(tokens);

        }

        public async Task DeleteToken(string tokenId)
        {
            await storageService.DeleteToken(tokenId);
            await this.LoadTokenList(force: true);
        }

        public async Task TokenToggleVisibility(string tokenId)
        {
            var all = TokenList ?? new();
            var token = all.Where(x => x.TokenId == tokenId).FirstOrDefault();
            if (token != null)
            {
                token.IsVisible = !token.IsVisible;
                await storageService.SaveTokenList(all.ToList());
                await this.LoadTokenList(force: true);
            }
        }

        public async Task AddToken(string tokenId, TokenData data, bool isUserAdded)
        {
            var newToken = await storageService.AddToken(tokenId, data, isUserAdded);
            var existing = TokenList.Where(x => x.TokenId == newToken.TokenId).FirstOrDefault();
            if (existing == null)
            {
                TokenList.Add(newToken);
            }
            else
            {
                existing = newToken;
            }
        }

        public async Task Clear()
        {
            await storageService.SaveTokenList(new());
            TokenList.Clear();
        }
    }
}
