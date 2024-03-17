namespace aoWebWallet.Models
{
    public enum ActivityLogType
    {
        None,
        ViewAddress,
        ViewToken,
        ViewTransaction,
        SendTransaction
    }

    public class LogData
    {
        public required string Id { get; set; }
        public int Count { get; set; }
        public DateTimeOffset LastAddDateTime { get; set; }
    }
}
