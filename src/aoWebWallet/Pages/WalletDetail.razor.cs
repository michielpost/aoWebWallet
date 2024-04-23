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
            //WatchObject(dataService.TokenList);
            WatchObject(BindingContext.BalanceDataList);
            WatchCollection(dataService.TokenList);
            WatchCollection(BindingContext.BalanceDataList);
            WatchDataLoaderVM(MainViewModel.WalletList);
            WatchDataLoaderVM(BindingContext.TokenTransferList);
            WatchDataLoaderVM(BindingContext.SelectedProcessData);

            dataService.TokenList.CollectionChanged += TokenList_CollectionChanged;

            base.OnInitialized();
        }

        private void TokenList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            BindingContext.TokenAddedRefresh();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Address != null && Address.Length != 43)
            {
                NavigationManager.NavigateTo("");
            }

            if (Address != null)
                await BindingContext.Initialize(Address);

            await base.OnParametersSetAsync();
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

        public override void Dispose()
        {
            dataService.TokenList.CollectionChanged -= TokenList_CollectionChanged;

            base.Dispose();
        }

    }
}
