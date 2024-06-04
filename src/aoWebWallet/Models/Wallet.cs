using aoWebWallet.Pages;
using System.Reflection.Metadata.Ecma335;

namespace aoWebWallet.Models
{
    public class Wallet
    {
        public required string Address { get; set; }
        public string? OwnerAddress { get; set; }
        public string? Name { get; set; }
        public string? Jwk { get; set; }
        public WalletTypes Source { get; set; }

        public bool IsReadOnly { get; set; }

        public DateTimeOffset? LastBackedUpDate { get; set; }
        public DateTimeOffset AddedDate { get; set; }
        public DateTimeOffset LastUsedDate { get; set; }

        public bool NeedsBackup => Source == WalletTypes.Generated && !string.IsNullOrEmpty(Jwk) && !LastBackedUpDate.HasValue;

    }

    public enum WalletTypes
    {
        None = 0,
        Manual,
        ArConnect,
        Explorer,
        Generated,
        Imported,
        AoProcess
    }
}
