using aoWebWallet.Pages;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;

namespace aoWebWallet.Models
{
    public class Wallet
    {
        public required string Address { get; set; }
        public string? OwnerAddress { get; set; }
        public string? Name { get; set; }

        /// <summary>
        /// Old unencrypted Jwk in browser storage
        /// </summary>
        public string? Jwk { get; set; }

        [JsonIgnore]
        public string? JwkSecret { get; set; }

        public string? JwkEncrypted { get; set; }
        public WalletTypes Source { get; set; }

        public bool IsReadOnly { get; set; }

        public DateTimeOffset? LastBackedUpDate { get; set; }
        public DateTimeOffset AddedDate { get; set; }
        public DateTimeOffset LastUsedDate { get; set; }

        [JsonIgnore]
        public bool NeedsUnlock => JwkEncrypted != null && JwkSecret == null;

        [JsonIgnore]
        public bool NeedsBackup => Source == WalletTypes.Generated && !string.IsNullOrEmpty(Jwk) && !LastBackedUpDate.HasValue;

        public string? GetJwkSecret()
        {
            return JwkSecret ?? Jwk;
        }
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
