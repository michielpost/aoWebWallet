using aoWebWallet.Models;
using aoWebWallet.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace aoWebWallet.Pages
{
    public partial class WalletDetail : MvvmComponentBase<WalletDetailViewModel>
    {
        protected override void OnInitialized()
        {
            WatchCollection(dataService.TokenList);
            WatchDataLoaderVM(MainViewModel.WalletList);
            WatchDataLoaderVM(BindingContext.BalanceDataList);
            WatchDataLoaderVM(BindingContext.TokenTransferList);
            WatchDataLoaderVM(BindingContext.SelectedProcessData);

            base.OnInitialized();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Address != null && Address.Length != 43)
            {
                NavigationManager.NavigateTo("");
            }

            if (Address != null)
                await BindingContext.Initialize(Address);

            base.OnParametersSetAsync();
        }

        protected override async Task LoadDataAsync()
        {
            dataService.LoadTokenList();

            //if (!string.IsNullOrEmpty(Address))
            //{
            //    BindingContext.LoadBalanceDataList(Address);
            //}

            await base.LoadDataAsync();
        }

        private async void DownloadWallet(Wallet wallet)
        {
            await MainViewModel.DownloadWallet(wallet);
            StateHasChanged();
        }

    }
}
