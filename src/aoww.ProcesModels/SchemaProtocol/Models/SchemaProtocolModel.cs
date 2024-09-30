using System.Text.Json.Serialization;

namespace aoww.ProcesModels.SchemaProtocol.Models
{
    public class SchemaProtocolModel : Dictionary<string, SchemaProtocolActionModel>
    {
      

    }

    public class SchemaProtocolActionModel
    {
        [JsonPropertyName("Title")]
        public string? Title { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        [JsonPropertyName("Schema")]
        public SchemaProtocolSchemaModel? Schema { get; set; }


    }

    public class SchemaProtocolSchemaModel
    {
        [JsonPropertyName("Tags")]
        public SchemaProtocolTagsModel? Tags { get; set; }

        //[JsonPropertyName("Data")]
        //public string? Data { get; set; }

    }



    public class SchemaProtocolTagsModel
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("required")]
        public List<string> Required { get; set; } = new();

        [JsonPropertyName("properties")]
        public Dictionary<string, SchemaProtocolPropertyModel> Properties { get; set; } = new();
    }

    public class SchemaProtocolPropertyModel
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("const")]
        public string? Const { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("minimum")]
        public int? Minimum { get; set; }

        [JsonPropertyName("maximum")]
        public int? Maximum { get; set; }

        [JsonPropertyName("$comment")]
        public string? Comment { get; set; }
   
        [JsonPropertyName("maxLength")]
        public int? MaxLength { get; set; }
    
        public List<string>? Enum { get; set; }
    }
}
