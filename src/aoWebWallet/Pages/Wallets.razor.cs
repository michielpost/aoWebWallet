using aoWebWallet.ViewModels;

namespace aoWebWallet.Pages
{
    public partial class Wallets : MvvmComponentBase<MainViewModel>
    {
        protected override void OnInitialized()
        {
            WatchDataLoaderVM(BindingContext.WalletList);

            base.OnInitialized();
        }

        protected override async Task LoadDataAsync()
        {
            await BindingContext.LoadWalletList();
            await BindingContext.LoadTokenList();

            //BindingContext.LoadStats();
        }

    }
}
