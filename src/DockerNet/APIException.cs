using System;
using System.Net;

namespace DockerNet
{
    public class APIException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public string ResponseBody { get; private set; }

        public APIException(HttpStatusCode statusCode, string responseBody)
            : base($"Docker API responded with status code={statusCode}, response={responseBody}")
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}
