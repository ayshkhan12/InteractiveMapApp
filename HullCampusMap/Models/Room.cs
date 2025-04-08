using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HullCampusMap.Models
{
    public class Room
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("floor")]
        public string Floor { get; set; } = string.Empty;

        [JsonPropertyName("pathData")]
        public string? PathData { get; set; }

        [JsonPropertyName("transform")]
        public string? Transform { get; set; }

        [JsonPropertyName("boundingBox")]
        public string? BoundingBox { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonIgnore]
        public string FloorName => Floor switch
        {
            "first.svg" => "First Floor",
            "second.svg" => "Second Floor",
            "third.svg" => "Third Floor",
            _ => "Unknown Floor"
        };

        [JsonIgnore]
        public string RoomNumber => Id.StartsWith("_") ? Id.TrimStart('_') : Id;

        [JsonPropertyName("alternateIds")]
        public List<string> AlternateIds { get; set; } = new();
    }
}