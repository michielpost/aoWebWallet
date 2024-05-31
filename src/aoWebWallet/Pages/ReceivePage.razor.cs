using aoWebWallet.ViewModels;

namespace aoWebWallet.Pages
{
    public partial class ReceivePage : MvvmComponentBase<ReceiveViewModel>
    {
        protected override void OnInitialized()
        {
            _timer = new Timer(Callback, null, 0, 10000);

            //WatchObject(BindingContext.Token);
            WatchDataLoaderVM(BindingContext.TokenTransferList);
            WatchObject(dataService.TokenDataLoader);

            WatchCollection(dataService.TokenList);
           

            base.OnInitialized();
        }

    }
}
