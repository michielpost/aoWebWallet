using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArweaveAO.Models.Token
{
    public class BalanceData
    {
        public string? TokenId { get; set; }
        public string? Account { get; set; }
        public string? Balance { get; set; }
        public string? Ticker { get; set; }
    }
}
