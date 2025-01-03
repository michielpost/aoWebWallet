﻿using aoWebWallet.Models;
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
            SetCanSend();
        }

        private void SetCanSend()
        {
            var jwk = Wallet.GetJwkSecret();

            var result = Wallet.Source switch
            {
                WalletTypes.None => false,
                WalletTypes.ArConnect => IsConnected,
                WalletTypes.Manual => !string.IsNullOrEmpty(Wallet.OwnerAddress) && OwnerCanSend,
                WalletTypes.Explorer => !string.IsNullOrEmpty(Wallet.OwnerAddress) && OwnerCanSend,
                WalletTypes.AoProcess => !string.IsNullOrEmpty(Wallet.OwnerAddress) && OwnerCanSend,
                WalletTypes.Generated => !string.IsNullOrEmpty(jwk),
                WalletTypes.Imported => !string.IsNullOrEmpty(jwk),
                _ => false
            };

            CanSend = result;

        }
    }
}
