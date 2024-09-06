using aoWebWallet.ViewModels;
using aoww.ProcesModels.Action;
using Microsoft.AspNetCore.Components.Routing;

namespace aoWebWallet.Pages
{
    public partial class CreateTokenPage : MvvmComponentBase<MainViewModel>
    {
        protected override void OnInitialized()
        {
            WatchProp(nameof(BindingContext.ActiveWalletAddress));
            WatchDataLoaderVM(BindingContext.WalletList);
            WatchDataLoaderVM(CreateTokenService.CreateTokenProgress);


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

    }
}
