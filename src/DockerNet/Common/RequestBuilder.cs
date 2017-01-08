using System;

namespace DockerNet.Common
{
    internal static class RequestBuilder
    {
        public static Uri Build(Uri baseUri, Version requestedApiVersion, string path, string queryString)
        {
            if (baseUri == null) throw new ArgumentNullException(nameof(baseUri));

            var builder = new UriBuilder(baseUri);

            if (requestedApiVersion != null)
                builder.Path += $"v{requestedApiVersion}/";

            if (!string.IsNullOrEmpty(path))
                builder.Path += path;

            if (queryString != null)
                builder.Query = queryString;

            return builder.Uri;
        }
    }
}
