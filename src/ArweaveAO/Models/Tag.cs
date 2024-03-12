using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ArweaveAO.Models
{
    [DebuggerDisplay("{Name}: {Value}")]
    public class Tag
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("value")]
        public required string Value { get; set; }
    }
}
