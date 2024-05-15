using aoWebWallet.ViewModels;

namespace aoWebWallet.Pages
{
    public partial class Tokens : MvvmComponentBase<MainViewModel>
    {
        protected override void OnInitialized()
        {
            WatchCollection(dataService.TokenList);
            WatchObject(dataService.TokenDataLoader);

            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await dataService.LoadTokenList();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

    }
}
