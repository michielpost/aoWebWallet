using aoWebWallet.ViewModels;
using System.Net;

namespace aoWebWallet.Pages
{
    public partial class TokenDetail : MvvmComponentBase<MainViewModel>
    {
        protected override void OnInitialized()
        {
            WatchDataLoaderVM(BindingContext.TokenList);
            WatchDataLoaderVM(BindingContext.TokenTransferList);

            base.OnInitialized();
        }

        protected override void OnParametersSet()
        {
            BindingContext.SelectedTokenId = null;
            if (TokenId != null && TokenId.Length != 43)
            {
                NavigationManager.NavigateTo("");
            }
            BindingContext.SelectedTokenId = TokenId;

            base.OnParametersSet();
        }

        protected override async Task LoadDataAsync()
        {
            await BindingContext.LoadTokenList();

            await base.LoadDataAsync();

        }

    }
}
