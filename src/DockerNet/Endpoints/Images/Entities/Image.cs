using System;
using System.Collections.Generic;

namespace DockerNet.Endpoints.Images.Entities
{
    public sealed class Image
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public IList<string> RepoTags { get; set; }
        public IList<string> RepoDigests { get; set; }
        public DateTime Created { get; set; }
        public long Size { get; set; }
        public long VirtualSize { get; set; }
        public IDictionary<string, string> Labels { get; set; }
    }
}
