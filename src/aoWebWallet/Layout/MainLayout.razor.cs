using aoWebWallet.Services;
using aoWebWallet.ViewModels;
using Microsoft.AspNetCore.Components;

namespace aoWebWallet.Layout
{
    public partial class MainLayout
    {
        [Inject]
        public MainViewModel BindingContext { get; set; } = default!;

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await BindingContext.LoadUserSettings();
                await BindingContext.CheckHasArConnectExtension();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        public virtual void Dispose()
        {
        }
    }
}
