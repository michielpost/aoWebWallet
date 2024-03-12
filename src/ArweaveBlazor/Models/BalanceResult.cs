using System;
using System.Text.Json.Serialization;

namespace ArweaveBlazor.Models
{
    public class BalanceResult
    {
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] 
        public long Balance { get; set; } = 0;
        public string Owner { get; set; } = default!;
        public string Symbol { get; set; } = default!;

    }
}
