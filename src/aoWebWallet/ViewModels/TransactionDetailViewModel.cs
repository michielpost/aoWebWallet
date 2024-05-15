using aoWebWallet.Models;
using aoWebWallet.Services;
using aoww.Services;
using aoww.Services.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using webvNext.DataLoader;

namespace aoWebWallet.ViewModels
{
    public class TransactionDetailViewModel : ObservableObject
    {
        private readonly MainViewModel mainViewModel;
        private readonly GraphqlClient graphqlClient;
        private readonly TokenDataService dataService;

        public DataLoaderViewModel<TokenTransfer> SelectedTransaction { get; set; } = new();
        public DataLoaderViewModel<List<TokenTransfer>> TokenTransferList { get; set; } = new();

        public TransactionDetailViewModel(MainViewModel mainViewModel, GraphqlClient graphqlClient, TokenDataService dataService)
        {
            this.mainViewModel = mainViewModel;
            this.graphqlClient = graphqlClient;
            this.dataService = dataService;
        }


        public async Task Initialize(string txId)
        {
            this.LoadSelectedTokenTransfer(txId);

            if (txId != null)
                mainViewModel.AddToLog(ActivityLogType.ViewTransaction, txId);
        }



        public Task LoadSelectedTokenTransfer(string txId) => SelectedTransaction.DataLoader.LoadAsync(async () =>
        {
            SelectedTransaction.Data = null;
            var result = await graphqlClient.GetTransactionsById(txId);

            SelectedTransaction.Data = result;

            if (result?.TokenId != null)
                dataService.TryAddTokenIds(new List<string>() { result.TokenId });

            return result;
        }, (x) => SelectedTransaction.Data = x);

    }
}
