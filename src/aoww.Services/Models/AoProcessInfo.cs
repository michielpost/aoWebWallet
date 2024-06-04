namespace aoww.Services.Models
{
    public class AoProcessInfo
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Version { get; set; }
        public string? Owner { get; set; }
    }
}
