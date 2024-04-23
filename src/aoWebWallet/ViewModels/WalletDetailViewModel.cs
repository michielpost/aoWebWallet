﻿using aoWebWallet.Models;
using aoWebWallet.Services;
using ArweaveAO;
using CommunityToolkit.Mvvm.ComponentModel;
using MudBlazor;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using webvNext.DataLoader;
using webvNext.DataLoader.Cache;
using static MudBlazor.Colors;

namespace aoWebWallet.ViewModels
{
    public partial class WalletDetailViewModel : ObservableObject
    {
        private readonly MainViewModel mainViewModel;
        private readonly GraphqlClient graphqlClient;
        private readonly TokenDataService dataService;
        private readonly TokenClient tokenClient;
        private readonly StorageService storageService;
        private readonly ISnackbar snackbar;
        private readonly MemoryDataCache memoryDataCache;

        private string? selectedAddress = null;


        [ObservableProperty]
        private bool canClaim1;
        [ObservableProperty]
        private bool canClaim2;
        [ObservableProperty]
        private bool canClaim3;

        public int? SelectedWalletIndex { get; set; }


        public WalletDetailsViewModel? SelectedWallet { get; set; }


        public DataLoaderViewModel<WalletProcessDataViewModel> SelectedProcessData { get; set; } = new();

        public ObservableCollection<BalanceDataViewModel> BalanceDataList { get; } = new();

        public DataLoaderViewModel<List<TokenTransfer>> TokenTransferList { get; set; } = new();


        public WalletDetailViewModel(MainViewModel mainViewModel, 
            GraphqlClient graphqlClient, 
            TokenDataService dataService,
            TokenClient tokenClient,
            StorageService storageService,
            ISnackbar snackbar,
            MemoryDataCache memoryDataCache)
        {
            this.mainViewModel = mainViewModel;
            this.graphqlClient = graphqlClient;
            this.dataService = dataService;
            this.tokenClient = tokenClient;
            this.storageService = storageService;
            this.snackbar = snackbar;
            this.memoryDataCache = memoryDataCache;
        }


        public async Task Initialize(string address)
        {
            selectedAddress = address;

            await SelectWallet(address);

            await LoadSelectedWalletProcessData(address);
            await LoadSelectedWalletOwnerData(address);

            mainViewModel.CheckHasArConnectExtension();

            SetClaims();

            mainViewModel.AddToLog(ActivityLogType.ViewAddress, address);
        }

        public async Task RefreshTokenTransferList()
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
                await LoadTokenTransferList(selectedAddress);
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
                    var indexOf = all.IndexOf(current);
                    SelectedWalletIndex = (indexOf % 5) + 1;

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
                    SelectedWalletIndex = 5;
                }

                this.LoadBalanceDataList(address);
                this.LoadTokenTransferList(address);

            }
            else
            {
                SelectedWallet = null;
                SelectedWalletIndex = null;
            }
        }

        public Task LoadTokenTransferList(string address) => TokenTransferList.DataLoader.LoadAsync(async () =>
        {
            TokenTransferList.Data = new();

            var incoming = await graphqlClient.GetTransactionsIn(address);
            var outgoing = await graphqlClient.GetTransactionsOut(address);
            var outgoingProcess = await graphqlClient.GetTransactionsOutFromProcess(address);

            var all = incoming.Concat(outgoing).Concat(outgoingProcess);

            TokenTransferList.Data = all.OrderByDescending(x => x.Timestamp).ToList();

            var allTokenIds = all.Select(x => x.TokenId).Distinct().ToList();
            dataService.TryAddTokenIds(allTokenIds);

            return TokenTransferList.Data;
        });



        public async Task LoadBalanceDataList(string address, bool onlyNew = false)
        {
            //First clear
            if (!onlyNew)
                BalanceDataList.Clear();

            var result = new List<BalanceDataViewModel>();

            foreach (var token in dataService.TokenList.Where(x => x.IsVisible))
            {
                if (onlyNew)
                {
                    if (BalanceDataList.Where(x => x.Token?.TokenId == token.TokenId).Any())
                        continue;

                }
                var balanceData = new BalanceDataViewModel { Token = token };

                balanceData.BalanceDataLoader.DataLoader.LoadAsync(async () =>
                {
                    var balanceData = await tokenClient.GetBalance(token.TokenId, address);
                    return balanceData;
                }, (x) =>
                {
                    balanceData.BalanceDataLoader.Data = x;
                    TokenTransferList.ForcePropertyChanged();
                });

                BalanceDataList.Add(balanceData);
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

        public async Task AddWalletAsReadonly()
        {
            if (SelectedWallet != null)
            {
                SelectedWallet.Wallet.Source = WalletTypes.Manual;
                if (SelectedWallet.Wallet.OwnerAddress != null)
                    SelectedWallet.Wallet.Source = WalletTypes.AoProcess;

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