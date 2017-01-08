using System.IO;

namespace DockerNet.Http
{
    public abstract class WriteClosableStream : Stream
    {
        public abstract bool CanCloseWrite { get; }

        public abstract void CloseWrite();
    }
}
