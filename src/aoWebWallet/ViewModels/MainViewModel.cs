using aoWebWallet.Models;
using aoWebWallet.Services;
using ArweaveAO.Models.Token;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using webvNext.DataLoader;
using webvNext.DataLoader.Cache;
using static MudBlazor.Colors;

namespace aoWebWallet.ViewModels
{
    public partial class MainViewModel : ObservableRecipient
    {
        private readonly DataService dataService;
        private readonly StorageService storageService;
        private readonly MemoryDataCache memoryDataCache;

        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        private string? computeUnitUrl;

        [ObservableProperty]
        private string? selectedAddress;

        [ObservableProperty]
        private Wallet? selectedWallet;

        public DataLoaderViewModel<List<Token>> TokenList { get; set; } = new();
        public DataLoaderViewModel<List<BalanceData>> BalanceDataList { get; set; } = new();
        public DataLoaderViewModel<List<Wallet>> WalletList { get; set; } = new();

        //TODO:
        //Actions List (optional? address)


        /// <summary>
        /// Gets the <see cref="IAsyncRelayCommand{T}"/> responsible for loading the source markdown docs.
        /// </summary>
        public MainViewModel(DataService dataService, StorageService storageService, MemoryDataCache memoryDataCache) : base()
        {
            this.dataService = dataService;
            this.storageService = storageService;
            this.memoryDataCache = memoryDataCache;
        }

        public Task LoadTokenList() => TokenList.DataLoader.LoadAsync(async () =>
        {
                var result = await dataService.LoadTokenData();
                return result;
        }, x => TokenList.Data = x);

        public Task LoadBalanceDataList(string address) => BalanceDataList.DataLoader.LoadAsync(async () =>
        {
            var result = await dataService.LoadWalletBalances(address);
            return result;
        }, x => BalanceDataList.Data = x);

        public async Task LoadWalletList()
        {
            var list = await storageService.GetWallets();
            WalletList.Data = list;
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
                    await this.LoadBalanceDataList(address);
                }
            }
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

    }
}
