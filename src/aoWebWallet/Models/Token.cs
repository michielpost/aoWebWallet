using ArweaveAO.Models.Token;

namespace aoWebWallet.Models
{
    public class Token
    {
        public required string TokenId { get; set; }
        public bool IsSystemToken { get; set; }
        public bool IsUserAdded { get; set; }
        public bool IsVisible { get; set; } = true;

        public TokenData? TokenData { get; set; }
    }
}
