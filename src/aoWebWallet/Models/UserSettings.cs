namespace aoWebWallet.Models
{
    public class UserSettings
    {
        //public bool? IsDarkMode { get; set; } = true;

        public GatewayUrlConfig GatewayUrlConfig { get; set; } = new();

        public bool Claimed1 { get; set; }
        public bool Claimed2 { get; set; }
        public bool Claimed3 { get; set; }

    }

    public class GatewayUrlConfig
    {
        public string GatewayUrl { get; set; } = "https://arweave.net";
        public string GraphqlUrl { get; set; } = "https://arweave.net/graphql";
        public string ComputeUnitUrl { get; set; } = "https://cu.ao-testnet.xyz";
        public string MessengerUnitUrl { get; set; } = "https://mu.ao-testnet.xyz";
    }


}
