using aoWebWallet.ViewModels;

namespace aoWebWallet.Pages
{
    public partial class Tokens : MvvmComponentBase<MainViewModel>
    {
        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected override async Task LoadDataAsync()
        {
            await dataService.LoadTokenList();

            await base.LoadDataAsync();
        }

    }
}
