using System;
using System.Collections.Generic;

namespace DockerNet.Endpoints.Images.Entities
{
    public class ImageHistory
    {
        public string Id { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public IList<string> Tags { get; set; }
        public long Size { get; set; }
        public string Comment { get; set; }
    }
}
