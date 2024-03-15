using aoWebWallet.ViewModels;
using MudBlazor;

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
            BindingContext.LoadTokenList();

            //if (!string.IsNullOrEmpty(Address))
            //{
            //    BindingContext.LoadBalanceDataList(Address);
            //}

            await base.LoadDataAsync();
        }

    }
}
