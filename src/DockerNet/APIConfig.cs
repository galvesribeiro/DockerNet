using System;

namespace DockerNet
{
    public sealed class APIConfig : IDisposable
    {
        public Uri Endpoint { get; private set; }

        public APICredentials Credentials { get; private set; }

        public APIConfig(Uri endpoint, APICredentials credentials = null)
        {
            Credentials = credentials ?? new UnsecureCredentials();
            Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        }

        public void Dispose()
        {
            Credentials?.Dispose();
            Credentials = null;
        }
    }
}
