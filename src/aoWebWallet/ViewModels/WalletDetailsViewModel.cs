using aoWebWallet.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace aoWebWallet.ViewModels
{
    public partial class WalletDetailsViewModel : ObservableObject
    {
        public WalletDetailsViewModel(Wallet wallet)
        {
            Wallet = wallet;
            SetCanSend();
        }

        public Wallet Wallet { get; set; }

        [ObservableProperty]
        private bool isConnected;

        [ObservableProperty]
        private bool ownerCanSend;

        [ObservableProperty]
        private bool canSend;

        partial void OnIsConnectedChanged(bool value)
        {
            SetCanSend();
        }

        partial void OnOwnerCanSendChanged(bool value)
        {
            Console.WriteLine("Owner can send: " + value);
            SetCanSend();
        }

        private void SetCanSend()
        {
            var result = Wallet.Source switch
            {
                WalletTypes.None => false,
                WalletTypes.ArConnect => IsConnected,
                WalletTypes.Manual => !string.IsNullOrEmpty(Wallet.OwnerAddress) && OwnerCanSend,
                WalletTypes.Explorer => !string.IsNullOrEmpty(Wallet.OwnerAddress) && OwnerCanSend,
                WalletTypes.AoProcess => !string.IsNullOrEmpty(Wallet.OwnerAddress) && OwnerCanSend,
                WalletTypes.Generated => !string.IsNullOrEmpty(Wallet.Jwk),
                WalletTypes.Imported => !string.IsNullOrEmpty(Wallet.Jwk),
                _ => false
            };

            CanSend = result;

        }
    }
}
