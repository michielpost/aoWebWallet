using aoWebWallet.ViewModels;
using System.Net;

namespace aoWebWallet.Pages
{
    public partial class TokenDetail : MvvmComponentBase<TokenDetailViewModel>
    {
        protected override void OnInitialized()
        {
            WatchCollection(dataService.TokenList);
            WatchDataLoaderVM(BindingContext.Token);
            WatchDataLoaderVM(BindingContext.TokenTransferList);

            base.OnInitialized();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (TokenId == null || TokenId.Length != 43)
            {
                NavigationManager.NavigateTo("");
            }

            if(TokenId != null)
                await BindingContext.Initialize(TokenId);

            base.OnParametersSetAsync();
        }

        protected override async Task LoadDataAsync()
        {
            //await dataService.LoadTokenList();

            await base.LoadDataAsync();

        }

    }
}
