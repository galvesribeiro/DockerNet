using System.Net;

namespace DockerNet.Endpoints.Images
{
    public class ImageNotFoundException : APIException
    {
        public ImageNotFoundException(HttpStatusCode statusCode, string body) : base(statusCode, body) { }
    }
}
