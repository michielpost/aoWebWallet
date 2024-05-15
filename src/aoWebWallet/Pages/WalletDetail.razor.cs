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
            //WatchObject(BindingContext.BalanceDataList);
            WatchObject(dataService.TokenDataLoader);

            WatchCollection(dataService.TokenList);
            WatchCollection(BindingContext.BalanceDataList);
            WatchDataLoaderVM(MainViewModel.WalletList);
            WatchDataLoaderVM(BindingContext.TokenTransferList);
            WatchDataLoaderVM(BindingContext.SelectedProcessData);

            dataService.TokenList.CollectionChanged += TokenList_CollectionChanged;
            BindingContext.PropertyChanged += BindingContext_PropertyChanged;

            base.OnInitialized();
        }

        private void BindingContext_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BindingContext.VisibleTokenList))
            {
                BindingContext.TokenAddedRefresh();
            }
        }

        private async void TokenList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await BindingContext.TokenAddedRefresh();
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
