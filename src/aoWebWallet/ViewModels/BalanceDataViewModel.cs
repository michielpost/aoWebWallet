using aoWebWallet.Models;
using ArweaveAO.Models.Token;
using webvNext.DataLoader;

namespace aoWebWallet.ViewModels
{
    public class BalanceDataViewModel
    {
        public DataLoaderViewModel<BalanceData> BalanceDataLoader { get; set; } = new DataLoaderViewModel<BalanceData>();
        public required Token Token { get; set; }

        public required string Address { get; set; }

        //public void Load()
        //{
        //    BalanceDataLoader.DataLoader.LoadAsync(async () =>
        //    {
        //        var balanceData = await tokenClient.GetBalance(token.TokenId, address);
        //        return balanceData;
        //    }, (x) =>
        //    {
        //        balanceData.BalanceDataLoader.Data = x;
        //        TokenTransferList.ForcePropertyChanged();
        //    });
        //}
    }
}
