using DockerNet.Endpoints.Images;
using System;

namespace DockerNet
{
    public interface IAPIClient : IDisposable
    {
        IImagesEndpoint Images { get; }

        //IContainerOperations Containers { get; }

        //IMiscellaneousOperations Miscellaneous { get; }
    }
}
