using System.IO;
using System.Net;
using System.Net.Http.Headers;

namespace DockerNet.Http
{
    internal class APIResponseStream
    {
        public HttpStatusCode StatusCode { get; private set; }

        public Stream Body { get; private set; }

        public HttpResponseHeaders Headers { get; private set; }

        public APIResponseStream(HttpStatusCode statusCode, Stream body, HttpResponseHeaders headers)
        {
            StatusCode = statusCode;
            Body = body;
            Headers = headers;
        }
    }
}
