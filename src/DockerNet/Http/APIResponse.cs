using System.Net;

namespace DockerNet.Http
{
    internal class APIResponse
    {
        public HttpStatusCode StatusCode { get; private set; }

        public string Body { get; private set; }

        public APIResponse(HttpStatusCode statusCode, string body)
        {
            StatusCode = statusCode;
            Body = body;
        }
    }
}
