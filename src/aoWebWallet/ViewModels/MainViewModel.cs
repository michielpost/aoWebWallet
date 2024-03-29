﻿using aoWebWallet.Models;
using aoWebWallet.Services;
using ArweaveAO;
using ArweaveAO.Models.Token;
using ArweaveBlazor;
using ClipLazor.Components;
using ClipLazor.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MudBlazor;
using webvNext.DataLoader;
using webvNext.DataLoader.Cache;

namespace aoWebWallet.ViewModels
{
    public partial class MainViewModel : ObservableRecipient
    {
        private const string CLAIM_PROCESS_ID = "5Mv1TBYZvKjNlWUpH78hWORIhqj1uqn_wdkJrA7emfU";

        private readonly DataService dataService;
        private readonly TokenClient tokenClient;
        private readonly StorageService storageService;
        private readonly ArweaveService arweaveService;
        private readonly GraphqlClient graphqlClient;
        private readonly ISnackbar snackbar;
        private readonly IClipLazor clipboard;
        private readonly MemoryDataCache memoryDataCache;

        [ObservableProperty]
        private bool isDarkMode = true;

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

        [ObservableProperty]
        private UserSettings? userSettings;

        [ObservableProperty]
        private bool canClaim1;
        [ObservableProperty]
        private bool canClaim2;
        [ObservableProperty]
        private bool canClaim3;

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
            ISnackbar snackbar,
            IClipLazor clipboard,
            MemoryDataCache memoryDataCache) : base()
        {
            this.dataService = dataService;
            this.tokenClient = tokenClient;
            this.storageService = storageService;
            this.arweaveService = arweaveService;
            this.graphqlClient = graphqlClient;
            this.snackbar = snackbar;
            this.clipboard = clipboard;
            this.memoryDataCache = memoryDataCache;
        }

        public async Task AddToLog(ActivityLogType type, string id)
        {
            await storageService.AddToLog(type, id);
            await SetClaims();
        }

        public Task LoadTokenTransferList(string address) => TokenTransferList.DataLoader.LoadAsync(async () =>
        {
            TokenTransferList.Data = new();

            var incoming = await graphqlClient.GetTransactionsIn(address);
            var outgoing = await graphqlClient.GetTransactionsOut(address);

            var all = incoming.Concat(outgoing);

            TokenTransferList.Data = all.OrderByDescending(x => x.Timestamp).ToList();

            var allTokenIds = all.Select(x => x.TokenId).Distinct().ToList();
            TryAddTokenIds(allTokenIds);

            return TokenTransferList.Data;
        });


        public Task LoadTokenTransferListForToken(string? tokenId) => TokenTransferList.DataLoader.LoadAsync(async () =>
        {
            TokenTransferList.Data = new();

            if (!string.IsNullOrWhiteSpace(tokenId))
            {
                var all = await graphqlClient.GetTransactionsForToken(tokenId);

                TokenTransferList.Data = all.ToList();

                var allTokenIds = all.Select(x => x.TokenId).Distinct().ToList();
                TryAddTokenIds(allTokenIds);

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

                if(result != null)
                    TryAddTokenIds(new List<string?>() { result.TokenId });

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

            foreach (var token in tokens.Where(x => x.IsVisible))
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

        public async Task AddToken(string tokenId, TokenData data, bool isUserAdded = false)
        {
            BalanceDataList.Data = null;
            await storageService.AddToken(tokenId, data, isUserAdded);
            await LoadTokenList();

            if(!string.IsNullOrEmpty(SelectedAddress))
            {
                await LoadBalanceDataList(SelectedAddress);
            }

            if (!string.IsNullOrEmpty(SelectedTransactionId))
            {
                await this.LoadSelectedTokenTransfer();
            }

            await this.SetClaims();
        }
        public async Task DeleteToken(string tokenId)
        {
            BalanceDataList.Data = null;
            await storageService.DeleteToken(tokenId);
            await this.LoadTokenList();
        }

        public async Task TokenToggleVisibility(string tokenId)
        {
            var all = await storageService.GetTokenIds();
            var token = all.Where(x => x.TokenId == tokenId).FirstOrDefault();
            if(token != null)
            {
                token.IsVisible = !token.IsVisible;
                await storageService.SaveTokenList(all);
                await this.LoadTokenList();
            }
        }

        public async Task SaveWallet(Wallet wallet)
        {
            await storageService.SaveWallet(wallet);
            await LoadWalletList();
            await SetClaims();
        }

        public async Task DeleteWallet(Wallet wallet)
        {
            await storageService.DeleteWallet(wallet);
            await LoadWalletList();
        }

        public async Task DownloadWallet(Wallet wallet)
        {
            if (string.IsNullOrEmpty(wallet.Jwk))
                return;

            var address = await arweaveService.GetAddress(wallet.Jwk);
            var result = await arweaveService.SaveFile($"{address}.json", wallet.Jwk);
            wallet.LastBackedUpDate = DateTimeOffset.UtcNow;

            if (this.WalletList.Data != null)
            {
                await storageService.SaveWalletList(this.WalletList.Data);
                //await LoadWalletList();
            }
        }

        public async Task ClearUserData()
        {
            memoryDataCache.Clear();

            await storageService.SaveTokenList(new());
            await storageService.SaveWalletList(new());
            await storageService.SaveUserSettings(new());
            await storageService.ClearActivityLog();

            //Clear all data
            TokenList.Data = null;
            WalletList = new();
            BalanceDataList.Data = null;
        }

        private async Task TryAddTokenIds(List<string?> allTokenIds)
        {
           foreach(var tokenId in allTokenIds)
            {
                if (string.IsNullOrEmpty(tokenId))
                    continue;

                var exist = TokenList.Data?.Where(x => x.TokenId == tokenId).Any() ?? false;
                if (exist)
                    continue;

                var data = await tokenClient.GetTokenMetaData(tokenId);
                if (data != null)
                {
                    await AddToken(tokenId, data, isUserAdded: false);
                }
            }
        }


        partial void OnSelectedAddressChanged(string? value)
        {
            SelectWallet(value);

            if (value != null)
                this.AddToLog(ActivityLogType.ViewAddress, value);
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
                        Name = null,
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


            if (value != null)
                this.AddToLog(ActivityLogType.ViewTransaction, value);
        }

        partial void OnSelectedTokenIdChanged(string? value)
        {
            this.LoadTokenTransferListForToken(value);


            if (value != null)
                this.AddToLog(ActivityLogType.ViewToken, value);
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

        public async Task LoadUserSettings()
        {
            UserSettings = await storageService.GetUserSettings();
            if (UserSettings != null)
            {
                IsDarkMode = UserSettings.IsDarkMode ?? true;
            }
        }

        public async Task SaveUserSettings()
        {
            if (UserSettings != null)
            {
                await storageService.SaveUserSettings(UserSettings);
                IsDarkMode = UserSettings.IsDarkMode ?? true;
            }
        }

        public async Task AddWalletAsReadonly()
        {
            if(SelectedWallet != null)
            {
                SelectedWallet.Source = WalletTypes.Manual;
                await storageService.SaveWallet(SelectedWallet);
                await LoadWalletList();

                snackbar.Add("Wallet added to list as read-only wallet.", Severity.Info);

            }
        }

        public async Task SetClaims()
        {
            var viewTokenActivity = await storageService.GetLog(ActivityLogType.ViewToken);
            var viewTransactionctivity = await storageService.GetLog(ActivityLogType.ViewTransaction);
            var viewAddressActivity = await storageService.GetLog(ActivityLogType.ViewAddress);
            var sendTransactionActivity = await storageService.GetLog(ActivityLogType.SendTransaction);

            CanClaim1 = sendTransactionActivity.Count > 0;
            CanClaim2 = CanClaim1 && sendTransactionActivity.Count > 1 && (WalletList.Data?.Count() > 1 || TokenList.Data?.Count() > 6);
            CanClaim3 = CanClaim2 && sendTransactionActivity.Count > 1 && WalletList.Data?.Count() > 2 && viewTokenActivity.Count > 0 && viewAddressActivity.Count > 2 && viewTransactionctivity.Count > 1;

            Console.WriteLine("1:" + CanClaim1);
        }

        public async Task Claim1()
        {
            if(UserSettings != null)
            {
                var tx = await Claim(1);
                if (tx != null && !string.IsNullOrEmpty(tx.Id))
                {
                    UserSettings.Claimed1 = true;
                    await storageService.SaveUserSettings(UserSettings);

                    snackbar.Add("Claim 1 successful. You received 10 AOWW!", Severity.Info);

                    if(SelectedAddress != null)
                        await LoadBalanceDataList(this.SelectedAddress);

                }
                else
                {
                    snackbar.Add("Claim was not successful.", Severity.Error);
                }
            }

        }
        public async Task Claim2()
        {
            if (UserSettings != null)
            {
                var tx = await Claim(2);
                if (tx != null && !string.IsNullOrEmpty(tx.Id))
                {
                    UserSettings.Claimed2 = true;
                    await storageService.SaveUserSettings(UserSettings);

                    snackbar.Add("Claim 2 successful. You received 20 AOWW!", Severity.Info);

                    if (SelectedAddress != null)
                        await LoadBalanceDataList(this.SelectedAddress);

                }
                else
                {
                    snackbar.Add("Claim was not successful.", Severity.Error);
                }
            }
        }
        public async Task Claim3()
        {
            if (UserSettings != null)
            {
                var tx = await Claim(3);
                if (tx != null && !string.IsNullOrEmpty(tx.Id))
                {
                    UserSettings.Claimed3 = true;
                    await storageService.SaveUserSettings(UserSettings);

                    snackbar.Add("Claim 3 successful. You received 30 AOWW!", Severity.Info);

                    if (SelectedAddress != null)
                        await LoadBalanceDataList(this.SelectedAddress);

                }
                else
                {
                    snackbar.Add("Claim was not successful.", Severity.Error);
                }
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

        public async Task CopyToClipboard(string? text)
        {
            bool isSupported = await clipboard.IsClipboardSupported();
            bool isWritePermitted = await clipboard.IsPermitted(PermissionCommand.Write);
            if (isSupported && !string.IsNullOrEmpty(text))
            {
                if (isWritePermitted)
                {
                    var isCopied = await clipboard.WriteTextAsync(text.AsMemory());
                    if (isCopied)
                    {
                        snackbar.Add("Address copied to clipboard", Severity.Success);
                    }
                }
            }
        }

        public Task<Transaction?> SendToken(Wallet wallet, string tokenId, string address, long amount)
        {
            if (wallet.Source == WalletTypes.ArConnect)
                return SendTokenWithArConnect(tokenId, address, amount);

            if (!string.IsNullOrEmpty(wallet.Jwk))
                return SendTokenWithJwk(wallet.Jwk, tokenId, address, amount);

            return Task.FromResult<Transaction?>(default);

        }

        public Task<Transaction?> SendTokenWithJwk(string jwk, string tokenId, string address, long amount)
           => LastTransactionId.DataLoader.LoadAsync(async () =>
           {
               var idResult = await arweaveService.SendAsync(jwk, tokenId, null, new List<ArweaveBlazor.Models.Tag>
           {
                new ArweaveBlazor.Models.Tag() { Name = "Action", Value = "Transfer"},
                new ArweaveBlazor.Models.Tag() { Name = "Wallet", Value = "aoww"},
                new ArweaveBlazor.Models.Tag() { Name = "Recipient", Value = address},
                new ArweaveBlazor.Models.Tag() { Name = "Quantity", Value = amount.ToString()},
           });

               return new Transaction { Id = idResult };
           });

        public Task<Transaction?> SendTokenWithArConnect(string tokenId, string address, long amount)
            => LastTransactionId.DataLoader.LoadAsync(async () =>
            {
                await CheckHasArConnectExtension();
                if (string.IsNullOrEmpty(ActiveArConnectAddress))
                    return null;

                var idResult = await arweaveService.SendAsync(null, tokenId, null, new List<ArweaveBlazor.Models.Tag>
            {
                new ArweaveBlazor.Models.Tag() { Name = "Action", Value = "Transfer"},
                new ArweaveBlazor.Models.Tag() { Name = "Wallet", Value = "aoww"},
                new ArweaveBlazor.Models.Tag() { Name = "Recipient", Value = address},
                new ArweaveBlazor.Models.Tag() { Name = "Quantity", Value = amount.ToString()},
            });

                return new Transaction { Id = idResult };
            });

        public Task<Transaction?> Claim(int claim)
            => LastTransactionId.DataLoader.LoadAsync(async () =>
            {
                await CheckHasArConnectExtension();
                if (string.IsNullOrEmpty(ActiveArConnectAddress))
                    return null;

                var idResult = await arweaveService.SendAsync(null, CLAIM_PROCESS_ID, null, new List<ArweaveBlazor.Models.Tag>
            {
                new ArweaveBlazor.Models.Tag() { Name = "Action", Value = "claim" + claim},
                new ArweaveBlazor.Models.Tag() { Name = "Wallet", Value = "aoww"},
            });

                return new Transaction { Id = idResult };
            });

        public async Task SetIsDarkMode(bool isDarkMode)
        {
            if(UserSettings != null)
            {
                UserSettings.IsDarkMode = isDarkMode;
                await SaveUserSettings();
            }
        }
    }
}
