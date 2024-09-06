namespace aoWebWallet.Models
{
    public class CreateTokenModel
    {
        public string? Name { get; set; }
        public string? Ticker { get; set; }
        public string? LogoUrl { get; set; }
        public int Denomination { get; set; }
        public int TotalSupply { get; set; }
    }
}
