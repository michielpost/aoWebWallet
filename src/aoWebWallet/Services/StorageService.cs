using aoWebWallet.Models;
using aoww.ProcesModels;
using ArweaveAO.Models.Token;
using Blazored.LocalStorage;

namespace aoWebWallet.Services
{
    public class StorageService
    {
        private readonly ILocalStorageService localStorage;

        private const string MEMFRAME_LIST_KEY = "MEMFRAME_LIST";
        private const string TOKEN_LIST_KEY = "TOKEN_LIST";
        private const string WALLET_LIST_KEY = "WALLET_LIST";
        private const string USER_SETTINGS_KEY = "USER_SETTINGS";
        public StorageService(ILocalStorageService localStorage)
        {
            this.localStorage = localStorage;
        }

        public async ValueTask<List<MemeFrameProcess>> GetMemeFrames()
        {
            var result = await localStorage.GetItemAsync<List<MemeFrameProcess>>(MEMFRAME_LIST_KEY);
            result = result ?? new();

            AddSystemMemeFrames(result,"OT9qTE2467gcozb2g8R6D6N3nQS94ENcaAIJfUzHCww", Constants.CredTokenId); //TRUNK
            AddSystemMemeFrames(result,"2gM9n9QO6JG1_bZhCWr3fuEKJtzRgx1xvYUB92nVFAs", Constants.CredTokenId); //AORTA
            AddSystemMemeFrames(result,"-a4T7XLMDGTcu8_preKXdUT6__4sJkMhYLEJZkXUYd0", Constants.CredTokenId); //MEME
            AddSystemMemeFrames(result,"rik3eCayInKVNzSMdoxeSEfpxNd5U7tx1H8NAveg4o8", Constants.CredTokenId); //FINCH-MEME
            //AddSystemMemeFrames(result, "kqvDWqWWJIaEWqarff3-Ux15sRG8sToh7X3b4SYh5Sc", "c2fBkprpp46V-PVn5ZdoevuKGz-Pt28EsqEkWMfnfrM"); //Test Meme

            return result;

        }

        private static void AddSystemMemeFrames(List<MemeFrameProcess> list, string processId, string mintTokenId)
        {
            var existing = list.Where(x => x.ProcessId == processId).FirstOrDefault();
            if (existing == null)
                list.Add(new MemeFrameProcess(processId) {  MintTokenId = mintTokenId });
        }

        public ValueTask SaveMemeFrames(List<MemeFrameProcess> list)
        {
            var uniqueItems = list.GroupBy(i => i.ProcessId).Select(g => g.First());

            return localStorage.SetItemAsync(MEMFRAME_LIST_KEY, uniqueItems);
        }

        public async ValueTask<List<Token>> GetTokenIds()
        {
            var result = await localStorage.GetItemAsync<List<Token>>(TOKEN_LIST_KEY);
            result = result ?? new();

            AddSystemTokens(result);

            return result;
        }

        public static void AddSystemTokens(List<Token> result)
        {
            AddSystemToken(result, Constants.AoTokenId,
                new TokenData
                {
                    TokenId = Constants.AoTokenId,
                    Denomination = 12,
                    Logo = "UkS-mdoiG8hcAClhKK8ch4ZhEzla0mCPDOix9hpdSFE",
                    Name = "AO",
                    Ticker = "AO"
                },
                proxyTokenId:Constants.AoProxyTokenId); //AO

            AddSystemToken(result, Constants.CredTokenId,
               new TokenData
               {
                   TokenId = Constants.CredTokenId,
                   Denomination = 3,
                   Logo = "eIOOJiqtJucxvB4k8a-sEKcKpKTh9qQgOV3Au7jlGYc",
                   Name = "AOCRED",
                   Ticker = "testnet-AOCRED"
               }); //CRED

            AddSystemToken(result, Constants.LlamaTokenId,
              new TokenData
              {
                  TokenId = Constants.LlamaTokenId,
                  Denomination = 12,
                  Logo = "9FSEgmUsrug7kTdZJABDekwTGJy7YG7KaN5khcbwcX4",
                  Name = "Llama Coin",
                  Ticker = "LLAMA"
              }); //LLAMA


            AddSystemToken(result, "8p7ApPZxC_37M06QHVejCQrKsHbcJEerd3jWNkDUWPQ",
                new TokenData
                {
                    TokenId = "8p7ApPZxC_37M06QHVejCQrKsHbcJEerd3jWNkDUWPQ",
                    Denomination = 3,
                    Logo = "AdFxCN1eEPboxNpCNL23WZRNhIhiamOeS-TUwx_Nr3Q",
                    Name = "Bark",
                    Ticker = "BRKTST"
                }); //BARK

            AddSystemToken(result, "OT9qTE2467gcozb2g8R6D6N3nQS94ENcaAIJfUzHCww",
                new TokenData
                {
                    TokenId = "OT9qTE2467gcozb2g8R6D6N3nQS94ENcaAIJfUzHCww",
                    Denomination = 3,
                    Logo = "4eTBOaxZSSyGbpKlHyilxNKhXbocuZdiMBYIORjS4f0",
                    Name = "TRUNK",
                    Ticker = "TRUNK"
                });  //TRUNK

            AddSystemToken(result, "aYrCboXVSl1AXL9gPFe3tfRxRf0ZmkOXH65mKT0HHZw",
              new TokenData
              {
                  TokenId = "aYrCboXVSl1AXL9gPFe3tfRxRf0ZmkOXH65mKT0HHZw",
                  Denomination = 6,
                  Logo = "wfI-5PlYXL66_BqquCXm7kq-ic1keu0b2CqRjw82yrU",
                  Name = "AR.IO EXP",
                  Ticker = "EXP"
              });

            AddSystemToken(result, "BUhZLMwQ6yZHguLtJYA5lLUa9LQzLXMXRfaq9FVcPJc",
                new TokenData
                {
                    TokenId = "BUhZLMwQ6yZHguLtJYA5lLUa9LQzLXMXRfaq9FVcPJc",
                    Denomination = 12,
                    Logo = "quMiswyIjELM0FZtjVSiUtg9_-pvQ8K25yfxrp1TgnQ",
                    Name = "0rbit Points",
                    Ticker = "0RBT"
                });  //0rbit

            AddSystemToken(result, "PBg5TSJPQp9xgXGfjN27GA28Mg5bQmNEdXH2TXY4t-A",
               new TokenData
               {
                   TokenId = "PBg5TSJPQp9xgXGfjN27GA28Mg5bQmNEdXH2TXY4t-A",
                   Denomination = 12,
                   Logo = "VzvP24VxdNt1kf3E-EXxxrihaNBnXpEI-5ymwWddJRk",
                   Name = "Earth",
                   Ticker = "EARTH"
               });

            AddSystemToken(result, "KmGmJieqSRJpbW6JJUFQrH3sQPEG9F6DQETlXNt4GpM",
               new TokenData
               {
                   TokenId = "KmGmJieqSRJpbW6JJUFQrH3sQPEG9F6DQETlXNt4GpM",
                   Denomination = 12,
                   Logo = "jayAVj1wgIcmin0bjG_DIGxq3_qANSp5EV7PcfUAvdQ",
                   Name = "Fire",
                   Ticker = "FIRE"
               });

            AddSystemToken(result, "2nfFJb8LIA69gwuLNcFQezSuw4CXPE4--U-j-7cxKOU",
               new TokenData
               {
                   TokenId = "2nfFJb8LIA69gwuLNcFQezSuw4CXPE4--U-j-7cxKOU",
                   Denomination = 12,
                   Logo = "7WqV5FWdDcbQzQNxNvfpr093yLHDtjeO7qPM9HQskWE",
                   Name = "Air",
                   Ticker = "AIR"
               });

            AddSystemToken(result, "NkXX3uZ4oGkQ3DPAWtjLb2sTA-yxmZKdlOlEHqMfWLQ",
               new TokenData
               {
                   TokenId = "NkXX3uZ4oGkQ3DPAWtjLb2sTA-yxmZKdlOlEHqMfWLQ",
                   Denomination = 12,
                   Logo = "ioI2_z6qkzGBrvZXbojjf6Q5uVZumx4rDDdHm-Jfyt0",
                   Name = "Lava",
                   Ticker = "FIRE-EARTH"
               });

            AddSystemToken(result, "GcFxqTQnKHcr304qnOcq00ZqbaYGDn4Wbb0DHAM-wvU",
              new TokenData
              {
                  TokenId = "GcFxqTQnKHcr304qnOcq00ZqbaYGDn4Wbb0DHAM-wvU",
                  Denomination = 12,
                  Logo = "K8nurc9H0_ZQm17jbs3ryEs6MrlX-oIK_krpprWlQ-Q",
                  Name = "Astro USD (Test)",
                  Ticker = "USDA-TST"
              });
        }

        private static void AddSystemToken(List<Token> list, string tokenId, TokenData tokenData, string? proxyTokenId = null)
        {
            var existing = list.Where(x => x.TokenId == tokenId).FirstOrDefault();
            if (existing != null)
            {
                existing.IsSystemToken = true;
                existing.TokenData = tokenData;
                existing.ProxyTokenId = proxyTokenId;
            }
            else
                list.Add(new Token { TokenId = tokenId, ProxyTokenId = proxyTokenId, IsSystemToken = true, TokenData = tokenData });
        }

        public async ValueTask<Token> AddToken(string tokenId, TokenData data, bool isUserAdded, bool? isVisible)
        {
            var list = await GetTokenIds();

            var existing = list.Where(x => x.TokenId == tokenId).FirstOrDefault();
            if (existing != null)
            {
                if(isVisible.HasValue)
                    existing.IsVisible = isVisible.Value;

                if(!existing.IsSystemToken)
                    existing.IsUserAdded = true;
            }
            else
            {
                existing = new Token { TokenId = tokenId, TokenData = data, IsUserAdded = isUserAdded, IsVisible = isVisible ?? true };
                list.Add(existing);
            }

            await SaveTokenList(list);

            return existing;
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
            var uniqueItems = list.GroupBy(i => i.TokenId).Select(g => g.First());

            return localStorage.SetItemAsync(TOKEN_LIST_KEY, uniqueItems);
        }

        public async ValueTask<List<Wallet>> GetWallets()
        {
            var result = await localStorage.GetItemAsync<List<Wallet>>(WALLET_LIST_KEY);
            return result ?? new();
        }


        public async ValueTask SaveWallet (Wallet wallet)
        {
            var list = await GetWallets();

            var existing = list.Where(x => x.Address == wallet.Address && x.IsReadOnly == wallet.IsReadOnly).FirstOrDefault();
            if(existing != null)
               list.Remove(existing);

            list.Insert(0,wallet);

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

            var ordered = all.OrderByDescending(x => x.LastAddDateTime).Take(15).ToList();

            await localStorage.SetItemAsync(type.ToString(), ordered);
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
