using aoWebWallet.Models;
using aoWebWallet.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace aoWebWallet.Pages
{
    public partial class WalletDetail : MvvmComponentBase<MainViewModel>
    {
        protected override void OnInitialized()
        {
            WatchDataLoaderVM(BindingContext.TokenList);
            WatchDataLoaderVM(BindingContext.WalletList);
            WatchDataLoaderVM(BindingContext.BalanceDataList);
            WatchDataLoaderVM(BindingContext.TokenTransferList);

            base.OnInitialized();
        }

        protected override void OnParametersSet()
        {
            BindingContext.SelectedWallet = null;
            //BindingContext.SelectedAddress = null;

            if(Address != null && Address.Length != 43)
            {
                NavigationManager.NavigateTo("");
            }

            BindingContext.SelectedAddress = Address;

            base.OnParametersSet();
        }

        protected override async Task LoadDataAsync()
        {
            await BindingContext.LoadTokenList();

            //if (!string.IsNullOrEmpty(Address))
            //{
            //    BindingContext.LoadBalanceDataList(Address);
            //}

            await base.LoadDataAsync();
        }

        private async void DownloadWallet(Wallet wallet)
        {
            await BindingContext.DownloadWallet(wallet);
            StateHasChanged();
        }

    }
}
