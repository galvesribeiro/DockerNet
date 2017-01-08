using System.Net.Http;

namespace DockerNet.Http
{
    internal interface IRequestContent
    {
        HttpContent GetContent();
    }
}
