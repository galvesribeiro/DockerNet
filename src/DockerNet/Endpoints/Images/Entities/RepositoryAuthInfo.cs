using Newtonsoft.Json;

namespace DockerNet.Endpoints.Images.Entities
{
    public class RepositoryAuthInfo
    {
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("auth")]
        public string Auth { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("serveraddress")]
        public string ServerAddress { get; set; }
        [JsonProperty("identitytoken")]
        public string IdentityToken { get; set; }
        [JsonProperty("registrytoken")]
        public string RegistryToken { get; set; }
    }
}
