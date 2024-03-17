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
            BindingContext.PropertyChanged += BindingContext_PropertyChanged;

            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                BindingContext.CheckHasArConnectExtension();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private void BindingContext_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.IsDarkMode))
            {
                this.StateHasChanged();
            }
        }

        public virtual void Dispose()
        {
            BindingContext.PropertyChanged -= BindingContext_PropertyChanged;
        }
    }
}
