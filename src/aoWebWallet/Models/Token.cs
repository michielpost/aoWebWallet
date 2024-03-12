using ArweaveAO.Models.Token;

namespace aoWebWallet.Models
{
    public class Token
    {
        public required string TokenId { get; set; }
        public bool IsSystemToken { get; set; }

        public TokenData? TokenData { get; set; }
    }
}
