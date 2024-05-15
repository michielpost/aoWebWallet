using aoWebWallet.ViewModels;

namespace aoWebWallet.Pages
{
    public partial class AddressBook : MvvmComponentBase<MainViewModel>
    {
        protected override void OnInitialized()
        {
            //WatchDataLoaderVM(BindingContext.TokenList);
            WatchDataLoaderVM(BindingContext.WalletList);
            WatchDataLoaderVM(BindingContext.ProcessesDataList);

            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await BindingContext.LoadWalletList();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

    }
}
