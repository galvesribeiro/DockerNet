using System;
using System.Collections.Generic;
using System.Text;

namespace DockerNet.Endpoints.Images.Entities
{
    public class Dockerfile
    {
        /// <summary>
        /// Path to the Dockerfile
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Path to the context directory
        /// </summary>
        public string ContextPath { get; set; }
        
        /// <summary>
        /// The image name (without tag)
        /// </summary>
        public string ImageName { get; set; }

        /// <summary>
        /// The tag for this image build.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// A Git repository URI or HTTP/HTTPS context URI. 
        /// If the URI points to a single text file, the file’s contents are placed into a file called Dockerfile and the image is built from that file. 
        /// If the URI points to a tarball, the file is downloaded by the daemon and the contents therein used as the context for the build. 
        /// If the URI points to a tarball and the dockerfile parameter is also specified, there must be a file with the corresponding path inside the tarball
        /// </summary>
        public string Remote { get; set; }

        /// <summary>
        /// Do not use the cache when building the image.
        /// </summary>
        public bool NoCache { get; set; }

        /// <summary>
        /// Attempt to pull the image even if an older image exists locally.
        /// </summary>
        public bool Pull { get; set; }

        /// <summary>
        /// Remove intermediate containers after a successful build. (default true)
        /// </summary>
        public bool RemoveIntermediate { get; set; } = true;

        /// <summary>
        /// Always remove intermediate containers.
        /// </summary>
        public bool AlwaysRemoveIntermediate { get; set; }

        /// <summary>
        /// Set memory limit for build.
        /// </summary>
        public long MemoryLimit { get; set; }

        /// <summary>
        /// Total memory (memory + swap), -1 to enable unlimited swap.
        /// </summary>
        public long TotalMemoryLimit { get; set; }

        /// <summary>
        /// CPU shares (relative weight).
        /// </summary>
        public long CPUShares { get; set; }

        /// <summary>
        /// CPUs in which to allow execution (e.g., 0-3, 0,1).
        /// </summary>
        public string CPUSetCPUs { get; set; }

        /// <summary>
        /// The length of a CPU period in microseconds.
        /// </summary>
        public long CPUPeriod { get; set; }

        /// <summary>
        /// Microseconds of CPU time that the container can get in a CPU period.
        /// </summary>
        public long CPUQuota { get; set; }

        /// <summary>
        /// Size of /dev/shm in bytes. The size must be greater than 0. If omitted the system uses 64MB
        /// </summary>
        public long SHMSize { get; set; }

        /// <summary>
        /// JSON map of string pairs for build-time variables.
        /// </summary>
        public IDictionary<string, string> BuildArgs { get; set; }

        /// <summary>
        /// JSON map of string pairs for labels to set on the image.
        /// </summary>
        public IDictionary<string, string> Labels { get; set; }
    }
}
