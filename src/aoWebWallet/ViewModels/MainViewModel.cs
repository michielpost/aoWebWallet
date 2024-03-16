using aoWebWallet.Models;
using aoWebWallet.Services;
using ArweaveAO;
using ArweaveAO.Models.Token;
using ArweaveBlazor;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Net;
using webvNext.DataLoader;
using webvNext.DataLoader.Cache;
using static MudBlazor.Colors;

namespace aoWebWallet.ViewModels
{
    public partial class MainViewModel : ObservableRecipient
    {
        private readonly DataService dataService;
        private readonly TokenClient tokenClient;
        private readonly StorageService storageService;
        private readonly ArweaveService arweaveService;
        private readonly GraphqlClient graphqlClient;
        private readonly MemoryDataCache memoryDataCache;

        [ObservableProperty]
        public bool? hasArConnectExtension;

        [ObservableProperty]
        public string? activeArConnectAddress;

        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        private string? computeUnitUrl;

        [ObservableProperty]
        private string? selectedAddress;

        [ObservableProperty]
        private Wallet? selectedWallet;

        [ObservableProperty]
        private int? selectedWalletIndex;

        [ObservableProperty]
        private BalanceDataViewModel? selectedBalanceDataVM;

        [ObservableProperty]
        private string? selectedTransactionId;

        [ObservableProperty]
        private string? selectedTokenId;

        public DataLoaderViewModel<Transaction> LastTransactionId { get; set; } = new();
        public DataLoaderViewModel<List<Token>> TokenList { get; set; } = new();
        public DataLoaderViewModel<List<DataLoaderViewModel<BalanceDataViewModel>>> BalanceDataList { get; set; } = new();
        public DataLoaderViewModel<List<Wallet>> WalletList { get; set; } = new();
        public DataLoaderViewModel<List<TokenTransfer>> TokenTransferList { get; set; } = new();
        public DataLoaderViewModel<TokenTransfer> SelectedTransaction { get; set; } = new();

        //TODO:
        //Actions List (optional? address)


        /// <summary>
        /// Gets the <see cref="IAsyncRelayCommand{T}"/> responsible for loading the source markdown docs.
        /// </summary>
        public MainViewModel(DataService dataService, 
            TokenClient tokenClient,
            StorageService storageService,
            ArweaveService arweaveService,
            GraphqlClient graphqlClient,
            MemoryDataCache memoryDataCache) : base()
        {
            this.dataService = dataService;
            this.tokenClient = tokenClient;
            this.storageService = storageService;
            this.arweaveService = arweaveService;
            this.graphqlClient = graphqlClient;
            this.memoryDataCache = memoryDataCache;
        }

        public Task LoadTokenTransferList(string address) => TokenTransferList.DataLoader.LoadAsync(async () =>
        {
            TokenTransferList.Data = new();

            var incoming = await graphqlClient.GetTransactionsIn(address);
            var outgoing = await graphqlClient.GetTransactionsOut(address);

            var all = incoming.Concat(outgoing);

            TokenTransferList.Data = all.OrderByDescending(x => x.Timestamp).ToList();

            return TokenTransferList.Data;
        });

        public Task LoadTokenTransferListForToken(string? tokenId) => TokenTransferList.DataLoader.LoadAsync(async () =>
        {
            TokenTransferList.Data = new();

            if (!string.IsNullOrWhiteSpace(tokenId))
            {
                var all = await graphqlClient.GetTransactionsForToken(tokenId);

                TokenTransferList.Data = all.ToList();

                return TokenTransferList.Data;
            }
            else
            {
                return null;
            }
        });

        public Task LoadTokenList() => TokenList.DataLoader.LoadAsync(async () =>
        {
            TokenList.Data = new();
            await foreach (var item in dataService.LoadTokenDataAsync())
            {
                TokenList.Data.Add(item);
                TokenList.ForcePropertyChanged();
            }

            return TokenList.Data;
        });

        public Task LoadSelectedTokenTransfer() => SelectedTransaction.DataLoader.LoadAsync(async () =>
        {
            SelectedTransaction.Data = null;
            if (!string.IsNullOrEmpty(SelectedTransactionId))
            {
                var result = await graphqlClient.GetTransactionsById(SelectedTransactionId);

                SelectedTransaction.Data = result;
                return result;
            }
            else
            {
                return null;
            }
        });


        public Task LoadBalanceDataList(string address) => BalanceDataList.DataLoader.LoadAsync(async () =>
        {
            //First clear
            BalanceDataList.Data = null;
            var tokens = await storageService.GetTokenIds();

            var result = new List<DataLoaderViewModel<BalanceDataViewModel>>();

            foreach (var token in tokens)
            {
                var balanceData = new DataLoaderViewModel<BalanceDataViewModel>();
                balanceData.Data = new BalanceDataViewModel {  Token = token };

                balanceData.DataLoader.LoadAsync(async () =>
                {
                    var balanceData = await tokenClient.GetBalance(token.TokenId, address);
                    return new BalanceDataViewModel() {  BalanceData = balanceData, Token = token };
                }, (x) => { balanceData.Data = x; BalanceDataList.ForcePropertyChanged(); });
                result.Add(balanceData);
            }

            BalanceDataList.Data = result;

            return result;

        });

        

        public async Task LoadWalletList()
        {
            var list = await storageService.GetWallets();
            WalletList.Data = list;
        }

        public async Task AddToken(string tokenId, TokenData data)
        {
            BalanceDataList.Data = null;
            await storageService.AddToken(tokenId, data);
            await LoadTokenList();

            if(!string.IsNullOrEmpty(SelectedAddress))
            {
                await LoadBalanceDataList(SelectedAddress);
            }
        }
        public async Task DeleteToken(string tokenId)
        {
            BalanceDataList.Data = null;
            await storageService.DeleteToken(tokenId);
            await this.LoadTokenList();
        }

        public async Task SaveWallet(Wallet wallet)
        {
            await storageService.SaveWallet(wallet);
            await LoadWalletList();
        }

        public async Task DeleteWallet(Wallet wallet)
        {
            await storageService.DeleteWallet(wallet);
            await LoadWalletList();
        }

        public async Task ClearUserData()
        {
            memoryDataCache.Clear();

            await storageService.SaveTokenList(new());
            await storageService.SaveWalletList(new());

            //Clear all data
            TokenList.Data = null;
            WalletList = new();
            BalanceDataList.Data = null;
        }

        partial void OnSelectedAddressChanged(string? value)
        {
            SelectWallet(value);
        }

        private async Task SelectWallet(string? address)
        {
            if (!string.IsNullOrEmpty(address))
            {
                var all = await storageService.GetWallets();
                var current = all.Where(x => x.Address == address).FirstOrDefault();
                if (current != null)
                {
                    SelectedWallet = current;
                    var indexOf = all.IndexOf(current);
                    SelectedWalletIndex = (indexOf % 5) + 1;
                   
                }
                else
                {
                    var tempWallet = new Wallet
                    {
                        Address = address,
                        AddedDate = DateTimeOffset.Now,
                        IsConnected = false,
                        IsReadOnly = true,
                        LastUsedDate = DateTimeOffset.UtcNow,
                        Name = address,
                        Source = WalletTypes.Explorer
                    };
                    SelectedWallet = tempWallet;
                    SelectedWalletIndex = 5;
                }

                await this.LoadBalanceDataList(address);
                await this.LoadTokenTransferList(address);
            }
            else
            {
                SelectedWallet = null;
                SelectedWalletIndex = null;
            }
        }

        partial void OnSelectedTransactionIdChanged(string? value)
        {
            this.LoadSelectedTokenTransfer();
        }

        partial void OnSelectedTokenIdChanged(string? value)
        {
            this.LoadTokenTransferListForToken(value);
        }

        partial void OnComputeUnitUrlChanged(string? value)
        {
            //ClearUserData();

            if (!string.IsNullOrEmpty(value))
            {
                //storageService.SetApiUrl(value);

                dataService.Init(value);
            }

        }

        public async Task DisconnectArWallet()
        {
            await arweaveService.DisconnectAsync();

            await CheckHasArConnectExtension();
        }

        public async Task CheckHasArConnectExtension()
        {
            HasArConnectExtension = await arweaveService.HasArConnectAsync();
            await GetActiveArConnectAddress();
        }

        public async Task GetActiveArConnectAddress()
        {
            if (this.WalletList.Data != null)
            {
                var wallets = this.WalletList.Data.Where(x => x.IsConnected && x.Source == WalletTypes.ArConnect);
                foreach (var wallet in wallets)
                {
                    wallet.IsConnected = false;
                }
                this.WalletList.ForcePropertyChanged();
                await storageService.SaveWalletList(this.WalletList.Data);
            }

            if (HasArConnectExtension.HasValue && HasArConnectExtension.Value)
            {
                ActiveArConnectAddress = await arweaveService.GetActiveAddress();

                if (this.WalletList.Data != null)
                {
                    var wallets = this.WalletList.Data.Where(x => !x.IsConnected
                        && x.Source == WalletTypes.ArConnect
                        && x.Address == ActiveArConnectAddress);
                    foreach (var wallet in wallets)
                    {
                        wallet.IsConnected = true;
                    }
                    this.WalletList.ForcePropertyChanged();
                    await storageService.SaveWalletList(this.WalletList.Data);
                }
            }
        }

        public Task<Transaction?> SendTokenWithArConnect(string tokenId, string address, long amount)
            => LastTransactionId.DataLoader.LoadAsync(async () =>
            {
                await CheckHasArConnectExtension();
                if (string.IsNullOrEmpty(ActiveArConnectAddress))
                    return null;

                var idResult = await arweaveService.SendAsync(tokenId, null, new List<ArweaveBlazor.Models.Tag>
            {
                new ArweaveBlazor.Models.Tag() { Name = "Action", Value = "Transfer"},
                new ArweaveBlazor.Models.Tag() { Name = "Recipient", Value = address},
                new ArweaveBlazor.Models.Tag() { Name = "Quantity", Value = amount.ToString()},
            });

                return new Transaction { Id = idResult };
            });
    }
}
