namespace aoWebWallet.Models
{
    public class Wallet
    {
        public required string Address { get; set; }
        public string? Name { get; set; }
        public WalletTypes Source { get; set; }
        public bool IsConnected { get; set; }
        public bool IsReadOnly { get; set; }

        public DateTimeOffset AddedDate { get; set; }
        public DateTimeOffset LastUsedDate { get; set; }
    }

    public enum WalletTypes
    {
        None = 0,
        Manual,
        ArConnect,
        Explorer
    }
}
