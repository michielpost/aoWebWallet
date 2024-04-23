using aoWebWallet.Models;
using ArweaveAO.Models.Token;
using webvNext.DataLoader;

namespace aoWebWallet.ViewModels
{
    public class BalanceDataViewModel
    {
        public DataLoaderViewModel<BalanceData> BalanceDataLoader { get; set; } = new DataLoaderViewModel<BalanceData>();
        public Token? Token { get; set; }
    }
}
