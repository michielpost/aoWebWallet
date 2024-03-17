using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aoWebWallet.Models
{
    public class TokenTransfer
    {
        public required string Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public int? BlockHeight { get; set; }
        public string? TokenId { get; set; }
        public required string From { get; set; }
        public string? To { get; set; }
        public long Quantity { get; set; }
    }
}
