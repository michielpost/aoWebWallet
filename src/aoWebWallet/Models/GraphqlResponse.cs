using ArweaveAO.Models;
using MudBlazor;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace aoWebWallet.Models
{
    public class GraphqlResponse
    {
        [JsonPropertyName("data")]
        public Data? Data { get; set; }

    }

    public class Block
    {
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("transactions")]
        public Transactions? Transactions { get; set; }
    }

    public class Edge
    {
        [JsonPropertyName("node")]
        public Node? Node { get; set; }
    }

    public class Node
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("recipient")]
        public string? Recipient { get; set; }

        [JsonPropertyName("owner")]
        public Owner? Owner { get; set; }

        [JsonPropertyName("block")]
        public Block? Block { get; set; }

        [JsonPropertyName("tags")]
        public List<Tag> Tags { get; set; } = new();
    }

    public class Owner
    {
        [JsonPropertyName("address")]
        public string Address { get; set; } = default!;
    }

    public class Transactions
    {
        [JsonPropertyName("edges")]
        public List<Edge> Edges { get; set; } = new();
    }
}
