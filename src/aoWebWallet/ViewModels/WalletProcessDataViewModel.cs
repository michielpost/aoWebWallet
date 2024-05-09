using aoww.Services.Models;
using ArweaveAO.Models.Token;

namespace aoWebWallet.ViewModels
{
    public class WalletProcessDataViewModel
    {
        public required string Address { get; set; }

        public List<AoProcessInfo> Processes { get; set; } = new();
    }
}
