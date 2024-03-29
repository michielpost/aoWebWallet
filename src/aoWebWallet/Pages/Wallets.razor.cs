using aoWebWallet.ViewModels;

namespace aoWebWallet.Pages
{
    public partial class Wallets : MvvmComponentBase<MainViewModel>
    {
        protected override void OnInitialized()
        {
            WatchDataLoaderVM(BindingContext.TokenList);
            WatchDataLoaderVM(BindingContext.WalletList);
            WatchDataLoaderVM(BindingContext.ProcessesDataList);

            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await BindingContext.CheckHasArConnectExtension();

                await BindingContext.LoadWalletList();
                await BindingContext.LoadTokenList();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        //protected override async Task LoadDataAsync()
        //{
           

        //    //BindingContext.LoadStats();
        //}

    }
}
