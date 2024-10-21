using aoWebWallet.ViewModels;

namespace aoWebWallet.Pages
{
    public partial class Wallets : MvvmComponentBase<MainViewModel>
    {
        protected override void OnInitialized()
        {
            //WatchDataLoaderVM(BindingContext.TokenList);
            WatchDataLoaderVM(BindingContext.WalletList);
            WatchDataLoaderVM(BindingContext.ProcessesDataList);
            WatchProp(nameof(BindingContext.SecretKey));

            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await BindingContext.LoadWalletList();
                await dataService.LoadTokenList();

                await BindingContext.CheckHasArConnectExtension();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

    }
}
