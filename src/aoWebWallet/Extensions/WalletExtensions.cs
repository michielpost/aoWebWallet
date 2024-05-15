using aoWebWallet.Models;

namespace aoWebWallet.Extensions
{
    public static class WalletExtensions
    {
        public static string ToAutocompleteDisplay(this Wallet wallet)
        {
            if (string.IsNullOrWhiteSpace(wallet.Name))
            {
                return wallet.Address;
            }

            return $"{wallet.Name} ({wallet.Address})";
        }
    }
}
