using aoWebWallet.Models;
using aoWebWallet.Pages;
using ArweaveAO.Models.Token;
using Blazored.LocalStorage;
using System.Reflection.Metadata;

namespace aoWebWallet.Services
{
    public class StorageService
    {
        private readonly ILocalStorageService localStorage;

        private const string TOKEN_LIST_KEY = "TOKEN_LIST";
        private const string WALLET_LIST_KEY = "WALLET_LIST";
        public StorageService(ILocalStorageService localStorage)
        {
            this.localStorage = localStorage;
        }

        public async ValueTask<List<Token>> GetTokenIds()
        {
            var result = await localStorage.GetItemAsync<List<Token>>(TOKEN_LIST_KEY);
            result = result ?? new();

            AddSystemToken(result, "Sa0iBLPNyJQrwpTTG-tWLQU-1QeUAJA73DdxGGiKoJc"); //CRED
            AddSystemToken(result, "8p7ApPZxC_37M06QHVejCQrKsHbcJEerd3jWNkDUWPQ"); //BARK
            AddSystemToken(result, "OT9qTE2467gcozb2g8R6D6N3nQS94ENcaAIJfUzHCww"); //TRUNK
            AddSystemToken(result, "BUhZLMwQ6yZHguLtJYA5lLUa9LQzLXMXRfaq9FVcPJc"); //0rbit

            return result;
        }

        private void AddSystemToken(List<Token> list, string tokenId)
        {
            var existing = list.Where(x => x.TokenId == tokenId).FirstOrDefault();
            if (existing != null)
                return;
            else
                list.Add(new Token { TokenId = tokenId, IsSystemToken = true });
        }

        public async ValueTask AddToken(string tokenId, TokenData data)
        {
            var list = await GetTokenIds();

            var existing = list.Where(x => x.TokenId == tokenId).FirstOrDefault();
            if (existing != null)
                return;
            else
                list.Add(new Token { TokenId = tokenId, TokenData = data});

            await SaveTokenList(list);
        }

        public async ValueTask DeleteToken(string tokenId)
        {
            var list = await GetTokenIds();

            var existing = list.Where(x => x.TokenId == tokenId && !x.IsSystemToken).FirstOrDefault();
            if (existing != null)
                list.Remove(existing);

            await SaveTokenList(list);
        }

        public ValueTask SaveTokenList(List<Token> list)
        {
            return localStorage.SetItemAsync(TOKEN_LIST_KEY, list);
        }

        public async ValueTask<List<Wallet>> GetWallets()
        {
            var result = await localStorage.GetItemAsync<List<Wallet>>(WALLET_LIST_KEY);
            return result ?? new();
        }

        public async ValueTask SaveWallet (Wallet wallet)
        {
            var list = await GetWallets();

            var existing = list.Where(x => x.Address == wallet.Address).FirstOrDefault();
            if(existing != null)
               list.Remove(existing);

            list.Add(wallet);

            await SaveWalletList(list);
        }

        public async ValueTask DeleteWallet(Wallet wallet)
        {
            var list = await GetWallets();

            var existing = list.Where(x => x.Address == wallet.Address).FirstOrDefault();
            if (existing != null)
                list.Remove(existing);

            await SaveWalletList(list);
        }

        public ValueTask SaveWalletList(List<Wallet> list)
        {
            return localStorage.SetItemAsync(WALLET_LIST_KEY, list);
        }
    }
}
