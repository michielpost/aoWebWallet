namespace aoWebWallet.Models
{
    public class UserSettings
    {
        public bool? IsDarkMode { get; set; } = true;
        public string? ComputeUnitUrl { get; set; }
        public string? GraphqlApiUrl { get; set; }

        public bool Claimed1 { get; set; }
        public bool Claimed2 { get; set; }
        public bool Claimed3 { get; set; }

    }

    
}
