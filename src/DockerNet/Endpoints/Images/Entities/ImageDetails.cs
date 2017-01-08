using DockerNet.Common;
using System;

namespace DockerNet.Endpoints.Images.Entities
{
    /// <summary>
    /// The result of a docker inspect operation
    /// </summary>
    public class ImageDetails
    {
        public string Id { get; set; }
        public string Container { get; set; }
        public string Comment { get; set; }
        public string OS { get; set; }
        public string OSVersion { get; set; }
        public string Architecture { get; set; }
        public string Parent { get; set; }
        public ConfigurationData ContainerConfig { get; set; }
        public string DockerVersion { get; set; }
        public long VirtualSize { get; set; }
        public long Size { get; set; }
        public string Author { get; set; }
        public DateTime Created { get; set; }
        public GraphDriverData GraphDriver { get; set; }
        public string[] RepoDigests { get; set; }
        public string[] RepoTags { get; set; }
        public ConfigurationData Config { get; set; }
        public FSData RootFS { get; set; }
    }
}
