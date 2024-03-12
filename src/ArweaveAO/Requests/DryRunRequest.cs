using ArweaveAO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ArweaveAO.Requests
{
    public class DryRunRequest
    {
        [JsonPropertyName("Id")]
        public string? Id { get; set; } = "0000000000000000000000000000000000000000001";

        [JsonPropertyName("Target")]
        public required string Target { get; set; }

        [JsonPropertyName("Owner")]
        public string Owner { get; set; } = "0000000000000000000000000000000000000000001";

        [JsonPropertyName("Anchor")]
        public string Anchor { get; set; } = "0";

        [JsonPropertyName("Data")]
        public string? Data { get; set; }

        [JsonPropertyName("Tags")]
        public List<Tag> Tags { get; set; } = new();
    }

}
