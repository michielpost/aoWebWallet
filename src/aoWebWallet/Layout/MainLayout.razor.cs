using aoWebWallet.Services;
using aoWebWallet.ViewModels;
using Microsoft.AspNetCore.Components;

namespace aoWebWallet.Layout
{
    public partial class MainLayout
    {
        [Inject]
        public MainViewModel BindingContext { get; set; } = default!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                BindingContext.CheckHasArConnectExtension();
            }

            await base.OnAfterRenderAsync(firstRender);
        }
    }
}
