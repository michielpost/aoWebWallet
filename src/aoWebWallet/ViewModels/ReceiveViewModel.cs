using aoWebWallet.Models;
using aoWebWallet.Services;
using aoww.ProcesModels;
using aoww.ProcesModels.Action;
using aoww.Services;
using aoww.Services.Models;
using ArweaveAO;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Net;
using System.Text;
using webvNext.DataLoader;

namespace aoWebWallet.ViewModels
{
    public partial class ReceiveViewModel(TokenDataService tokenDataService,
        GraphqlClient graphqlClient
        ) : ObservableObject
    {
        [ObservableProperty]
        private Token? _token;

        public DataLoaderViewModel<List<TokenTransfer>> TokenTransferList { get; set; } = new();

        public string? TokenId { get; set; }
        public DateTimeOffset StartTime { get; set; } = DateTimeOffset.Now;
        public required string Address { get; set; }
        public string QrCode
        {
            get
            {
                //return $"{Address}";

                if (string.IsNullOrEmpty(TokenId))
                    return $"ao:{Address}";
                else
                    return $"ao:{Address}?tokenId={TokenId}";
            }
        }

        public string ShareLink
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(TokenId))
                {
                    return "/action?" + TokenProcess.CreateForTokenTransaction(Address, TokenId).ToQueryString();
                }
                else
                    return $"/wallet/{Address}";
            }
        }

        public async Task Initialize(string address, string? tokenId)
        {
            Address = address;
            Token = null;
            TokenId = tokenId;
            StartTime = DateTimeOffset.UtcNow;
            TokenTransferList.Data?.Clear();

            if (!string.IsNullOrWhiteSpace(tokenId))
            {
                Token = await tokenDataService.LoadTokenAsync(tokenId);
            }

            await LoadTokenTransferList();
        }

        public Task LoadTokenTransferList() => TokenTransferList.DataLoader.LoadAsync(async () =>
        {
            var address = Address;

            var incoming = await graphqlClient.GetTokenTransfers(address);

            incoming = incoming.Where(x => x.Timestamp > StartTime).OrderByDescending(x => x.Timestamp).ToList();

            if(!string.IsNullOrEmpty(TokenId))
                incoming = incoming.Where(x => x.TokenId == TokenId).ToList();

            TokenTransferList.Data = incoming.OrderByDescending(x => x.Timestamp).ToList();

            List<string> allTokenIds = incoming.Where(x => x.TokenId != null).Select(x => x.TokenId!).Distinct().ToList();
            tokenDataService.TryAddTokenIds(allTokenIds);

            return TokenTransferList.Data;
        });
    }
}
