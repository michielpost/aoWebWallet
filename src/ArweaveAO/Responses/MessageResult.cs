using ArweaveAO.Models;
using System.Text.Json.Serialization;

namespace ArweaveAO.Responses
{
    public class MessageResult
    {
        [JsonPropertyName("Messages")]
        public List<Message> Messages { get; set; } = new();

        //[JsonPropertyName("Spawns")]
        //public List<object> Spawns { get; set; }

        //[JsonPropertyName("Output")]
        //public List<object> Output { get; set; }

        //[JsonPropertyName("GasUsed")]
        //public int GasUsed { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("Target")]
        public string? Target { get; set; }

        [JsonPropertyName("Anchor")]
        public string? Anchor { get; set; }

        [JsonPropertyName("Data")]
        public string? Data { get; set; }

        [JsonPropertyName("Tags")]
        public List<Tag> Tags { get; set; } = new();
    }
}
