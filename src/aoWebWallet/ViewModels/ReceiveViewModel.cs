using aoWebWallet.Models;
using aoWebWallet.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Net;

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
                    return "/action?" + AoAction.CreateForTokenTransaction(Address, TokenId).ToQueryString();
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

            if (!string.IsNullOrWhiteSpace(tokenId))
            {
                Token = await tokenDataService.LoadTokenAsync(tokenId);
            }

        }
    }
}
