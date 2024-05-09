using aoWebWallet.Models;
using aoWebWallet.Services;
using aoww.Services;
using aoww.Services.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using webvNext.DataLoader;
using static MudBlazor.Colors;

namespace aoWebWallet.ViewModels
{
    public class TokenDetailViewModel : ObservableObject
    {
        private readonly MainViewModel mainViewModel;
        private readonly GraphqlClient graphqlClient;
        private readonly TokenDataService dataService;

        public DataLoaderViewModel<Token> Token { get; set; } = new();

        public DataLoaderViewModel<List<TokenTransfer>> TokenTransferList { get; set; } = new();

        public TokenDetailViewModel(MainViewModel mainViewModel, GraphqlClient graphqlClient, TokenDataService dataService)
        {
            this.mainViewModel = mainViewModel;
            this.graphqlClient = graphqlClient;
            this.dataService = dataService;
        }


        public async Task Initialize(string tokenId)
        {
            await this.LoadTokenData(tokenId);

            this.LoadTokenTransferListForToken(tokenId);

            mainViewModel.AddToLog(ActivityLogType.ViewToken, tokenId);
        }

        public Task LoadTokenData(string tokenId) => Token.DataLoader.LoadAsync(async () =>
        {
            Token.Data = null;

            var result = await dataService.LoadTokenAsync(tokenId);

            return result;

        }, (x) => Token.Data = x);

        public Task LoadTokenTransferListForToken(string tokenId) => TokenTransferList.DataLoader.LoadAsync(async () =>
        {
            TokenTransferList.Data = new();

            var all = await graphqlClient.GetTransactionsForToken(tokenId);

            TokenTransferList.Data = all.ToList();

            var allTokenIds = all.Select(x => x.TokenId).Distinct().ToList();
            dataService.TryAddTokenIds(allTokenIds);

            return TokenTransferList.Data;
           
        });

    }
}
