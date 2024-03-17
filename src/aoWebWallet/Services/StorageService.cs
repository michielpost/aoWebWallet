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
        private const string USER_SETTINGS_KEY = "USER_SETTINGS";
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

        public ValueTask SaveUserSettings(UserSettings settings)
        {
            return localStorage.SetItemAsync(USER_SETTINGS_KEY, settings);
        }

        public async ValueTask<UserSettings> GetUserSettings()
        {
            var result = await localStorage.GetItemAsync<UserSettings>(USER_SETTINGS_KEY);
            return result ?? new();
        }

        public async Task AddToLog(ActivityLogType type, string id)
        {
            var all = await GetLog(type);
            var existing = all.Where(x => x.Id == id).FirstOrDefault();
            if (existing == null)
            {
                existing = new LogData() { Id = id };
                all.Add(existing);
            }
            existing.Count++;
            existing.LastAddDateTime = DateTimeOffset.UtcNow;

            await localStorage.SetItemAsync(type.ToString(), all);
        }

        public async ValueTask<List<LogData>> GetLog(ActivityLogType type)
        {
            var result = await localStorage.GetItemAsync<List<LogData>>(type.ToString());
            return result ?? new();
        }

        public async Task ClearActivityLog()
        {
            var values = Enum.GetValues(typeof(ActivityLogType)).Cast<ActivityLogType>();
            foreach (var val in values)
            {
                await localStorage.RemoveItemAsync(val.ToString());
            }
        }

    }
}
