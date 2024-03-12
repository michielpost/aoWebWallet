using aoWebWallet.ViewModels;

namespace aoWebWallet.Pages
{
    public partial class Tokens : MvvmComponentBase<MainViewModel>
    {
        protected override void OnInitialized()
        {
            WatchDataLoaderVM(BindingContext.TokenList);

            base.OnInitialized();
        }

        protected override async Task LoadDataAsync()
        {
            await BindingContext.LoadTokenList();

            await base.LoadDataAsync();

        }

    }
}
