using aoWebWallet.ViewModels;

namespace aoWebWallet.Pages
{
    public partial class WalletDetail : MvvmComponentBase<MainViewModel>
    {
        protected override void OnInitialized()
        {
            WatchDataLoaderVM(BindingContext.WalletList);
            WatchDataLoaderVM(BindingContext.BalanceDataList);

            base.OnInitialized();
        }

        protected override void OnParametersSet()
        {
            BindingContext.SelectedWallet = null;
            BindingContext.SelectedAddress = Address;

            base.OnParametersSet();
        }

        protected override async Task LoadDataAsync()
        {
            await BindingContext.LoadWalletList();

            //BindingContext.LoadStats();
        }

    }
}
