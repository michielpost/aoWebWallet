using aoWebWallet.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net;

namespace aoWebWallet.Pages
{
    public partial class TransactionDetail : MvvmComponentBase<MainViewModel>
    {
        [Parameter]
        public string? TxId { get; set; }

        protected override void OnInitialized()
        {
            WatchDataLoaderVM(BindingContext.TokenTransferList);
            WatchDataLoaderVM(BindingContext.SelectedTransaction);

            base.OnInitialized();
        }

        protected override void OnParametersSet()
        {
            BindingContext.SelectedTransactionId = null;

            if (TxId != null && TxId.Length != 43)
            {
                NavigationManager.NavigateTo("");
            }

            BindingContext.SelectedTransactionId = this.TxId;

            base.OnParametersSet();
        }

        protected override async Task LoadDataAsync()
        {
            await BindingContext.LoadTokenList();

            //if (!string.IsNullOrEmpty(Address))
            //{
            //    BindingContext.LoadBalanceDataList(Address);
            //}

            await base.LoadDataAsync();
        }

    }
}
