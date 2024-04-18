using aoWebWallet.Models;
using aoWebWallet.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net;

namespace aoWebWallet.Pages
{
    public partial class TransactionDetail : MvvmComponentBase<TransactionDetailViewModel>
    {
        [Parameter]
        public string? TxId { get; set; }

        protected override void OnInitialized()
        {
            WatchDataLoaderVM(BindingContext.TokenTransferList);
            WatchDataLoaderVM(BindingContext.SelectedTransaction);

            base.OnInitialized();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (TxId != null && TxId.Length != 43)
            {
                NavigationManager.NavigateTo("");
            }

            if (TxId != null)
                await BindingContext.Initialize(TxId);

            base.OnParametersSetAsync();
        }

        protected override async Task LoadDataAsync()
        {
            await dataService.LoadTokenList();

            //if (!string.IsNullOrEmpty(Address))
            //{
            //    BindingContext.LoadBalanceDataList(Address);
            //}

            await base.LoadDataAsync();
        }

    }
}
