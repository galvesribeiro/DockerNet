using DockerNet.Endpoints.Images.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DockerNet.Endpoints.Images
{
    public interface IImagesEndpoint
    {
        Task<IList<Image>> ListImages(ListImagesQuery query);
        Task BuildImage(Dockerfile dockerFile, IProgress<BuildImageProgress> progress, RepositoryAuthInfo authInfo = null);
        Task Pull(string image, IProgress<PullImageProgress> progress, string tag = "", RepositoryAuthInfo authInfo = null);
        Task<ImageDetails> Inspect(string image);
        Task<ImageHistory> History(string image);
        Task Push(string image, IProgress<PushImageProgress> progress, string tag = "", RepositoryAuthInfo authInfo = null);
        Task Tag(string sourceImage, string targetImage, string targetTag);
        Task<IList<IDictionary<string, string>>> Delete(string image, bool force = false, bool noPrune = false);
        Task<IList<ImageOverview>> Search(SearchImageQuery query);
    }
}
