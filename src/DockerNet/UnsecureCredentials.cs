using System.Net.Http;

namespace DockerNet
{
    public class UnsecureCredentials : APICredentials
    {
        public override HttpMessageHandler GetHandler(HttpMessageHandler innerHandler) => innerHandler;

        public override bool IsTlsEnabled() => false;
    }
}
