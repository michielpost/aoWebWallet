using aoWebWallet.Models;
using aoWebWallet.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace aoWebWallet.ViewModels
{
    public partial class ReceiveViewModel(TokenDataService tokenDataService) : ObservableObject
    {
        [ObservableProperty]
        private Token? _token;

        public string? TokenId { get; set; }
        public required string Address { get; set; }
        public string QrCode
        {
            get
            {
                return $"{Address}";

                if (string.IsNullOrEmpty(TokenId))
                    return $"ao:{Address}";
                else
                    return $"ao:{Address}?tokenId={TokenId}";
            }
        }

        public async Task Initialize(string address, string? tokenId)
        {
            Address = address;
            Token = null;
            TokenId = tokenId;

            if (!string.IsNullOrWhiteSpace(tokenId))
            {
                Token = await tokenDataService.LoadTokenAsync(tokenId);
            }

        }
    }
}
