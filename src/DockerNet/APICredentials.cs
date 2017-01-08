using System;
using System.Net.Http;

namespace DockerNet
{
    public abstract class APICredentials : IDisposable
    {
        public abstract bool IsTlsEnabled();

        public abstract HttpMessageHandler GetHandler(HttpMessageHandler innerHandler);

        public virtual void Dispose()
        {
        }
    }
}
