using aoWebWallet.Models;
using aoWebWallet.ViewModels;
using Microsoft.AspNetCore.Components.Routing;

namespace aoWebWallet.Pages
{
    public partial class ActionPage : MvvmComponentBase<MainViewModel>
    {
        public AoAction AoAction { get; set; } = new();

        protected override void OnInitialized()
        {
            transactionService.Reset();

            GetQueryStringValues();
            //WatchDataLoaderVM(BindingContext.TokenList);
            WatchDataLoaderVM(BindingContext.WalletList);
            WatchDataLoaderVM(transactionService.LastTransaction);

            //Auto select wallet
            if (!string.IsNullOrEmpty(WalletDetailViewModel.SelectedWallet?.Wallet.Address))
            {
                selectedWalletObj = WalletDetailViewModel.SelectedWallet?.Wallet;
                selectedWallet = selectedWalletObj?.Address;

            }

            NavigationManager.LocationChanged += NavigationManager_LocationChanged;

            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await BindingContext.CheckHasArConnectExtension();

                await BindingContext.LoadWalletList();
                //await dataService.LoadTokenList();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private void NavigationManager_LocationChanged(object? sender, LocationChangedEventArgs e)
        {
            GetQueryStringValues();
            StateHasChanged();
        }

        private async void GetQueryStringValues()
        {
            var uri = new Uri(NavigationManager.Uri);
            var query = uri.Query;

            AoAction = AoAction.CreateFromQueryString(query);

            //Add and load tokens
            var tokens = AoAction
                            .AllInputs
                            .Where(x => x.ParamType == ActionParamType.Balance || x.ParamType == ActionParamType.Quantity)
                            .Select(x => x.Args.FirstOrDefault())
                            .Distinct()
            .ToList();

            await dataService.TryAddTokenIds(tokens);

            StateHasChanged();
        }

       

        public override void Dispose()
        {
            NavigationManager.LocationChanged -= NavigationManager_LocationChanged;

            base.Dispose();
        }

        //protected override async Task LoadDataAsync()
        //{
        //    await BindingContext.LoadTokenList();

        //    await base.LoadDataAsync();

        //}

    }
}
