using aoWebWallet.Models;
using aoWebWallet.Services;
using aoww.Services;
using aoww.Services.Models;
using ArweaveAO;
using ArweaveBlazor;
using CommunityToolkit.Mvvm.ComponentModel;
using MudBlazor;
using System.Collections.ObjectModel;
using webvNext.DataLoader;
using webvNext.DataLoader.Cache;

namespace aoWebWallet.ViewModels
{
    public partial class WalletDetailViewModel : ObservableObject
    {
        private readonly MainViewModel mainViewModel;
        private readonly GraphqlClient graphqlClient;
        private readonly TokenDataService dataService;
        private readonly TokenClient tokenClient;
        private readonly StorageService storageService;
        private readonly ArweaveService arweaveService;
        private readonly ISnackbar snackbar;
        private readonly MemoryDataCache memoryDataCache;

        private List<TokenTransfer> incoming = new();
        private List<TokenTransfer> outgoing = new();
        private List<TokenTransfer> outgoingProcess = new();


        [ObservableProperty]
        public List<string> visibleTokenList = new();



        private string? selectedAddress = null;

        public bool CanLoadMoreTransactions { get; set; } = true;

        [ObservableProperty]
        private bool canClaim1;
        [ObservableProperty]
        private bool canClaim2;
        [ObservableProperty]
        private bool canClaim3;

        [ObservableProperty]
        public string? activeArConnectAddress;

        [ObservableProperty]
        public bool? hasArConnectExtension;

        public WalletDetailsViewModel? SelectedWallet { get; set; }


        public DataLoaderViewModel<WalletProcessDataViewModel> SelectedProcessData { get; set; } = new();

        public ObservableCollection<BalanceDataViewModel> BalanceDataList { get; } = new();

        public DataLoaderViewModel<List<TokenTransfer>> TokenTransferList { get; set; } = new();


        public WalletDetailViewModel(MainViewModel mainViewModel, 
            GraphqlClient graphqlClient, 
            TokenDataService dataService,
            TokenClient tokenClient,
            StorageService storageService,
            ArweaveService arweaveService,
            ISnackbar snackbar,
            MemoryDataCache memoryDataCache)
        {
            this.mainViewModel = mainViewModel;
            this.graphqlClient = graphqlClient;
            this.dataService = dataService;
            this.tokenClient = tokenClient;
            this.storageService = storageService;
            this.arweaveService = arweaveService;
            this.snackbar = snackbar;
            this.memoryDataCache = memoryDataCache;
        }


        public async Task Initialize(string address)
        {
            VisibleTokenList = new();
            VisibleTokenList.Add(Constants.AoTokenId); //AO
            VisibleTokenList.Add("Sa0iBLPNyJQrwpTTG-tWLQU-1QeUAJA73DdxGGiKoJc"); //TESTNET-CRED

            ResetTokenTransferlist();

            selectedAddress = address;

            await SelectWallet(address);

            await LoadSelectedWalletProcessData(address);
            await LoadSelectedWalletOwnerData(address);

            CheckHasArConnectExtension();

            SetClaims();

            mainViewModel.AddToLog(ActivityLogType.ViewAddress, address);
        }

        public async Task CheckHasArConnectExtension()
        {
            HasArConnectExtension = await arweaveService.HasArConnectAsync();
            await GetActiveArConnectAddress();
        }

        public async Task GetActiveArConnectAddress()
        {
            if (HasArConnectExtension.HasValue && HasArConnectExtension.Value)
            {
                ActiveArConnectAddress = await arweaveService.GetActiveAddress();

                if (this.SelectedWallet != null)
                {
                    this.SelectedWallet.IsConnected = SelectedWallet.Wallet.Address == ActiveArConnectAddress;
                }
            }
        }

        public async Task RefreshTokenTransferList()
        {
            if (selectedAddress != null)
            {
                ResetTokenTransferlist();

                await LoadTokenTransferList(selectedAddress);
            }
        }

        private void ResetTokenTransferlist()
        {
            incoming = new();
            outgoing = new();
            outgoingProcess = new();
            TokenTransferList.Data = new();
        }

        public async Task LoadMoreTransactions()
        {
            if (selectedAddress != null)
            {
                await LoadTokenTransferList(selectedAddress);
            }
        }

        public async Task RefreshBalanceDataList()
        {
            if (selectedAddress != null)
            {
                await LoadBalanceDataList(selectedAddress);
            }
        }

        public async Task RefreshBalance()
        {
            if (selectedAddress != null)
            {
                await RefreshTokenTransferList();
                await LoadBalanceDataList(selectedAddress);
            }
        }

        public async Task TokenAddedRefresh()
        {
            if (selectedAddress != null)
            {
                await LoadBalanceDataList(selectedAddress, onlyNew: true);
            }
        }

        //public async Task Refresh()
        //{
        //    if (selectedAddress != null)
        //        await Initialize(selectedAddress);
        //}

        private async Task SelectWallet(string? address)
        {
            if (!string.IsNullOrEmpty(address))
            {
                if (mainViewModel.WalletList.Data == null)
                {
                    await mainViewModel.LoadWalletList();
                }

                var all = mainViewModel.WalletList.Data ?? new();
                var current = all.Where(x => x.Address == address).FirstOrDefault();
                if (current != null)
                {
                    SelectedWallet = new WalletDetailsViewModel(current);
                }
                else
                {
                    var tempWallet = new Wallet
                    {
                        Address = address,
                        AddedDate = DateTimeOffset.Now,
                        LastUsedDate = DateTimeOffset.UtcNow,
                        Name = null,
                        Source = WalletTypes.Explorer
                    };
                    SelectedWallet = new WalletDetailsViewModel(tempWallet);
                }

                this.LoadBalanceDataList(address);
                this.LoadTokenTransferList(address);

                if (this.SelectedWallet != null)
                {
                    this.SelectedWallet.IsConnected = SelectedWallet.Wallet.Address == ActiveArConnectAddress;
                }

            }
            else
            {
                SelectedWallet = null;
            }
        }

        public Task LoadTokenTransferList(string address) => TokenTransferList.DataLoader.LoadAsync(async () =>
        {
            incoming = await graphqlClient.GetTokenTransfersIn(address, GetCursor(incoming));
            outgoing = await graphqlClient.GetTransactionsOut(address, GetCursor(outgoing));
            outgoingProcess = await graphqlClient.GetTransactionsOutFromProcess(address, GetCursor(outgoingProcess));

            var allNew = incoming.Concat(outgoing).Concat(outgoingProcess).OrderByDescending(x => x.Timestamp).ToList();
            CanLoadMoreTransactions = allNew.Any();

            var existing = TokenTransferList.Data ?? new();

            TokenTransferList.Data = existing.Concat(allNew).OrderByDescending(x => x.Timestamp).ToList();

            List<string> allTokenIds = allNew.Where(x => x.TokenId != null).Select(x => x.TokenId!).Distinct().ToList();
            dataService.TryAddTokenIds(allTokenIds);

            bool hasNew = false;
            foreach(var token in allTokenIds)
            {
                var exist = VisibleTokenList.Where(x => x == token).Any();
                if (!exist)
                {
                    VisibleTokenList.Add(token);
                    hasNew = true;
                }
            }
            if (hasNew)
                OnPropertyChanged(nameof(VisibleTokenList));


            return TokenTransferList.Data;
        });

        private static string? GetCursor(List<TokenTransfer> transactions)
        {
            return transactions.Select(x => x.Cursor).LastOrDefault();
        }

        private async Task LoadBalanceDataList(string address, bool onlyNew = false)
        {
            //First clear
            if (!onlyNew)
                BalanceDataList.Clear();

            foreach (var token in dataService.TokenList.Where(x => VisibleTokenList.Contains(x.TokenId) && x.IsVisible))
            {
                if (onlyNew)
                {
                    if (BalanceDataList.Where(x => x.Token?.TokenId == token.TokenId).Any())
                        continue;

                }
                var balanceData = new BalanceDataViewModel { Token = token, Address = address };
                BalanceDataList.Add(balanceData);

                await Task.Delay(50);

                balanceData.BalanceDataLoader.DataLoader.LoadAsync(async () =>
                {
                    var balanceData = await tokenClient.GetBalance(token.TokenId, address);
                    return balanceData;
                }, (x) =>
                {
                    balanceData.BalanceDataLoader.Data = x;
                    TokenTransferList.ForcePropertyChanged();
                });



            }
        }

        public async Task LoadSelectedWalletProcessData(string address)
        {
            SelectedProcessData.Data = new WalletProcessDataViewModel { Address = address };

            SelectedProcessData.DataLoader.LoadAsync(() =>
            {
                return memoryDataCache!.GetAsync($"{nameof(MainViewModel.LoadProcessesDataList)}-{address}", async () =>
                {
                    var data = await graphqlClient.GetAoProcessesForAddress(address);
                    return new WalletProcessDataViewModel() { Address = address, Processes = data };
                }, TimeSpan.FromMinutes(1));


            }, (x) => { SelectedProcessData.Data = x; });
        }

        public async Task LoadSelectedWalletOwnerData(string address)
        {
            var ownerAddress = await memoryDataCache!.GetAsync($"{nameof(LoadSelectedWalletOwnerData)}-{address}", async () =>
            {
                var data = await graphqlClient.GetOwnerForAoProcessAddress(address);
                return data?.Owner;
            }, TimeSpan.FromMinutes(1));

            if (SelectedWallet != null)
                SelectedWallet.Wallet.OwnerAddress = ownerAddress;

            CheckCanOwnerOfSelectedWalletSend();
        }

        private void CheckCanOwnerOfSelectedWalletSend()
        {
            if (!string.IsNullOrEmpty(SelectedWallet?.Wallet.OwnerAddress))
            {
                var owner = mainViewModel.WalletList.Data?.Where(x => x.Address == SelectedWallet.Wallet.OwnerAddress).FirstOrDefault();
                if (owner != null)
                {
                    var details = new WalletDetailsViewModel(owner);
                    var canOwnerSend = details?.CanSend ?? false;
                    SelectedWallet.OwnerCanSend = canOwnerSend;
                }
            }
        }

        public async Task SaveExplorerWallet()
        {
            if (SelectedWallet?.Wallet != null)
            {
                var existing = mainViewModel.WalletList.Data?.Where(x => x.Address == SelectedWallet.Wallet.Address).Any() ?? false;
                if (existing)
                    return;

                SelectedWallet.Wallet.Source = WalletTypes.Manual;
                SelectedWallet.Wallet.IsReadOnly = true;

                var ownerAddress = SelectedWallet.Wallet.OwnerAddress;
                if (ownerAddress != null)
                {
                    var ownerWallet = mainViewModel.WalletList.Data?.Where(x => !x.IsReadOnly && x.Address == ownerAddress).FirstOrDefault();

                    if (ownerWallet != null)
                    {
                        SelectedWallet.Wallet.Source = WalletTypes.AoProcess;
                        SelectedWallet.Wallet.IsReadOnly = false;
                    }
                }

                await storageService.SaveWallet(SelectedWallet.Wallet);
                await mainViewModel.LoadWalletList(force: true);

                snackbar.Add("Wallet added to list.", Severity.Info);

            }
        }

       


        public async Task SetClaims()
        {
            var viewTokenActivity = await storageService.GetLog(ActivityLogType.ViewToken);
            var viewTransactionctivity = await storageService.GetLog(ActivityLogType.ViewTransaction);
            var viewAddressActivity = await storageService.GetLog(ActivityLogType.ViewAddress);
            var sendTransactionActivity = await storageService.GetLog(ActivityLogType.SendTransaction);

            CanClaim1 = sendTransactionActivity.Count > 0;
            CanClaim2 = CanClaim1 && sendTransactionActivity.Count > 1 && (mainViewModel.WalletList.Data?.Count() > 1 || dataService.TokenList.Count() > 6);
            CanClaim3 = CanClaim2 && sendTransactionActivity.Count > 1 && mainViewModel.WalletList.Data?.Count() > 2 && viewTokenActivity.Count > 0 && viewAddressActivity.Count > 2 && viewTransactionctivity.Count > 1;

        }

        public async Task Claim1()
        {
            if (mainViewModel.UserSettings != null)
            {
                var tx = await mainViewModel.Claim(1);
                if (tx != null && !string.IsNullOrEmpty(tx.Id))
                {
                    mainViewModel.UserSettings.Claimed1 = true;
                    await storageService.SaveUserSettings(mainViewModel.UserSettings);

                    snackbar.Add("Claim 1 successful. You received 10 AOWW!", Severity.Info);

                    if (SelectedWallet != null)
                        await LoadBalanceDataList(this.SelectedWallet.Wallet.Address);

                }
                else
                {
                    snackbar.Add("Claim was not successful.", Severity.Error);
                }
            }

        }
        public async Task Claim2()
        {
            if (mainViewModel.UserSettings != null)
            {
                var tx = await mainViewModel.Claim(2);
                if (tx != null && !string.IsNullOrEmpty(tx.Id))
                {
                    mainViewModel.UserSettings.Claimed2 = true;
                    await storageService.SaveUserSettings(mainViewModel.UserSettings);

                    snackbar.Add("Claim 2 successful. You received 20 AOWW!", Severity.Info);

                    if (SelectedWallet != null)
                        await LoadBalanceDataList(this.SelectedWallet.Wallet.Address);

                }
                else
                {
                    snackbar.Add("Claim was not successful.", Severity.Error);
                }
            }
        }
        public async Task Claim3()
        {
            if (mainViewModel.UserSettings != null)
            {
                var tx = await mainViewModel.Claim(3);
                if (tx != null && !string.IsNullOrEmpty(tx.Id))
                {
                    mainViewModel.UserSettings.Claimed3 = true;
                    await storageService.SaveUserSettings(mainViewModel.UserSettings);

                    snackbar.Add("Claim 3 successful. You received 30 AOWW!", Severity.Info);

                    if (SelectedWallet != null)
                        await LoadBalanceDataList(this.SelectedWallet.Wallet.Address);

                }
                else
                {
                    snackbar.Add("Claim was not successful.", Severity.Error);
                }
            }
        }

        
    }
}
