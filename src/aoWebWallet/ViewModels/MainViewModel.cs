using aoWebWallet.Extensions;
using aoWebWallet.Models;
using aoWebWallet.Pages;
using aoWebWallet.Services;
using ArweaveAO;
using ArweaveAO.Models.Token;
using ArweaveBlazor;
using ClipLazor.Components;
using ClipLazor.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MudBlazor;
using System.Text.Json;
using webvNext.DataLoader;
using webvNext.DataLoader.Cache;

namespace aoWebWallet.ViewModels
{
    public partial class MainViewModel : ObservableRecipient
    {
        private const string CLAIM_PROCESS_ID = "5Mv1TBYZvKjNlWUpH78hWORIhqj1uqn_wdkJrA7emfU";

        private readonly TokenDataService dataService;
        private readonly TokenClient tokenClient;
        private readonly StorageService storageService;
        private readonly ArweaveService arweaveService;
        private readonly GraphqlClient graphqlClient;
        private readonly ISnackbar snackbar;
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
        private UserSettings? userSettings;


        public DataLoaderViewModel<Transaction> LastTransactionId { get; set; } = new();
        public DataLoaderViewModel<List<Wallet>> WalletList { get; set; } = new();
        public DataLoaderViewModel<List<DataLoaderViewModel<WalletProcessDataViewModel>>> ProcessesDataList { get; set; } = new();


        /// <summary>
        /// Gets the <see cref="IAsyncRelayCommand{T}"/> responsible for loading the source markdown docs.
        /// </summary>
        public MainViewModel(TokenDataService dataService,
            TokenClient tokenClient,
            StorageService storageService,
            ArweaveService arweaveService,
            GraphqlClient graphqlClient,
            ISnackbar snackbar,
            MemoryDataCache memoryDataCache) : base()
        {
            this.dataService = dataService;
            this.tokenClient = tokenClient;
            this.storageService = storageService;
            this.arweaveService = arweaveService;
            this.graphqlClient = graphqlClient;
            this.snackbar = snackbar;
            this.memoryDataCache = memoryDataCache;
        }

        public async Task AddToLog(ActivityLogType type, string id)
        {
            await storageService.AddToLog(type, id);
        }

        
        public Task LoadProcessesDataList() => ProcessesDataList.DataLoader.LoadAsync(async () =>
        {
            ProcessesDataList.Data = null;

            var result = new List<DataLoaderViewModel<WalletProcessDataViewModel>>();

            foreach (var wallet in WalletList.Data ?? new())
            {
                var address = wallet.Address;
                var processData = new DataLoaderViewModel<WalletProcessDataViewModel>();
                processData.Data = new WalletProcessDataViewModel { Address = address };

                processData.DataLoader.LoadAsync(() =>
                {
                    return memoryDataCache!.GetAsync($"{nameof(LoadProcessesDataList)}-{address}", async () =>
                    {
                        var data = await graphqlClient.GetAoProcessesForAddress(address);
                        return new WalletProcessDataViewModel() { Address = address, Processes = data };
                    }, TimeSpan.FromMinutes(1));


                }, (x) => { processData.Data = x; ProcessesDataList.ForcePropertyChanged(); });
                result.Add(processData);
            }

            ProcessesDataList.Data = result;

            return result;

        });

      

        public async Task LoadWalletList(bool force = false)
        {
            if (WalletList.Data == null || force)
            {
                var list = await storageService.GetWallets();

                //foreach (var wallet in list)
                //{
                //    if(wallet.Source == WalletTypes.AoProcess && !string.IsNullOrEmpty(wallet.OwnerAddress))
                //    {
                //        var owner = list.Where(x => x.Address == wallet.OwnerAddress).FirstOrDefault();
                //        var canOwnerSend = owner?.CanSend ?? false;
                //        wallet.OwnerCanSend = canOwnerSend;
                //    }
                //}

                WalletList.Data = list;

                await LoadProcessesDataList();
            }
        }

        public async Task SaveWallet(Wallet wallet)
        {
            await storageService.SaveWallet(wallet);
            await LoadWalletList(force: true);
        }

        public async Task DeleteWallet(Wallet wallet)
        {
            await storageService.DeleteWallet(wallet);
            await LoadWalletList(force: true);
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
                var selected = this.WalletList.Data.Where(x => x.Address == address).FirstOrDefault();
                if (selected != null)
                {
                    selected.LastBackedUpDate = DateTimeOffset.UtcNow;
                    await storageService.SaveWalletList(this.WalletList.Data);
                    //await LoadWalletList();
                }
            }
        }

        public async Task ClearUserData()
        {
            memoryDataCache.Clear();

            await dataService.Clear();
            await storageService.SaveWalletList(new());
            await storageService.SaveUserSettings(new());
            await storageService.ClearActivityLog();

            //Clear all data
            WalletList = new();
            //BalanceDataList.Data = null;
        }

        


      
       

        partial void OnComputeUnitUrlChanged(string? value)
        {
            //ClearUserData();

            if (!string.IsNullOrEmpty(value))
            {
                //storageService.SetApiUrl(value);

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
            if (HasArConnectExtension.HasValue && HasArConnectExtension.Value)
            {
                ActiveArConnectAddress = await arweaveService.GetActiveAddress();

                //if (this.SelectedWallet != null)
                //{
                //    this.SelectedWallet.IsConnected = SelectedWallet.Wallet.Address == ActiveArConnectAddress;
                //}
            }
        }

     

        public Task<Transaction?> SendToken(Wallet wallet, string tokenId, string address, long amount)
        {
            if (wallet.Source == WalletTypes.ArConnect)
                return SendTokenWithArConnect(tokenId, address, amount);

            if (!string.IsNullOrEmpty(wallet.OwnerAddress))
            {
                var ownerWallet = WalletList.Data!.Where(x => x.Address == wallet.OwnerAddress).FirstOrDefault();
                return SendTokenWithEval(ownerWallet?.Jwk, wallet.Address, tokenId, address, amount);

            }

            if (!string.IsNullOrEmpty(wallet.Jwk))
                return SendTokenWithJwk(wallet.Jwk, tokenId, address, amount);

            return Task.FromResult<Transaction?>(default);

        }

        public Task<Transaction?> SendTokenWithEval(string? jwk, string processId, string tokenId, string address, long amount)
          => LastTransactionId.DataLoader.LoadAsync(async () =>
          {

              var transferTags = new List<ArweaveBlazor.Models.Tag>
              {
                    new ArweaveBlazor.Models.Tag() { Name = "Target", Value = tokenId},
                    new ArweaveBlazor.Models.Tag() { Name = "Action", Value = "Transfer"},
                    new ArweaveBlazor.Models.Tag() { Name = "Wallet", Value = "aoww"},
                    new ArweaveBlazor.Models.Tag() { Name = "Recipient", Value = address},
                    new ArweaveBlazor.Models.Tag() { Name = "Quantity", Value = amount.ToString()},
              };



              var data = $"Send({transferTags.ToSendCommand()})";

              var evalTags = new List<ArweaveBlazor.Models.Tag>
              {
                    new ArweaveBlazor.Models.Tag() { Name = "Action", Value = "Eval"},
                    new ArweaveBlazor.Models.Tag() { Name = "Wallet", Value = "aoww"},
              };

              var idResult = await arweaveService.SendAsync(jwk, processId, data, evalTags);

              return new Transaction { Id = idResult };
          });

        public Task<Transaction?> SendTokenWithJwk(string? jwk, string tokenId, string address, long amount)
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

                return await SendTokenWithJwk(null, tokenId, address, amount);
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
            if (UserSettings != null)
            {
                UserSettings.IsDarkMode = isDarkMode;
                await SaveUserSettings();
            }
        }

    }
}
