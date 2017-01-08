using Newtonsoft.Json;

namespace DockerNet.Endpoints.Images.Entities
{
    public class ImageOverview
    {
        public string Name { get; set; }
        [JsonProperty("is_official")]
        public bool Official { get; set; }
        [JsonProperty("is_automated")]
        public bool Automated { get; set; }
        public string Description { get; set; }
        [JsonProperty("star_count")]
        public int Stars { get; set; }
    }
}
