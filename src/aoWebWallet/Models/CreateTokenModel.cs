using aoWebWallet.Services;

namespace aoWebWallet.Models
{
    public class CreateTokenModel
    {
        public string? Name { get; set; }
        public string? Ticker { get; set; }
        public string? LogoUrl { get; set; }
        public int Denomination { get; set; } = 2;
        public decimal TotalSupply { get; set; } = 100;
        public string MintQuantityForTag => BalanceHelper.DecimalToTokenAmount(TotalSupply, Denomination).ToString();

    }
}
