using aoWebWallet.ViewModels;

namespace aoWebWallet.Pages
{
    public partial class ReceivePage : MvvmComponentBase<ReceiveViewModel>
    {
        protected override void OnInitialized()
        {
            //WatchObject(BindingContext.Token);
            WatchObject(dataService.TokenDataLoader);

            WatchCollection(dataService.TokenList);
           

            base.OnInitialized();
        }

    }
}
