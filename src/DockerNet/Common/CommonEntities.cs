using System.Collections.Generic;

namespace DockerNet.Common
{
    public class GraphDriverData
    {
        public string Name { get; set; }
        public IDictionary<string, string> Data { get; set; }
    }

    public class FSData
    {
        public string Type { get; set; }
        public string[] Layers { get; set; }
    }
    public class ConfigurationData
    {
        public bool TTY { get; set; }
        public string Hostname { get; set; }
        public string[] Volumes { get; set; }
        public string DomainName { get; set; }
        public bool AttachStdout { get; set; }
        public string PublishService { get; set; }
        public bool AttachStdin { get; set; }
        public bool OpenStdin { get; set; }
        public bool StdinOnce { get; set; }
        public bool NetworkDisabled { get; set; }
        public string[] OnBuild { get; set; }
        public string Image { get; set; }
        public string User { get; set; }
        public string WorkingDir { get; set; }
        public string Entrypoint { get; set; }
        public string MacAddress { get; set; }
        public bool AttachStderr { get; set; }
        public IDictionary<string, string> Labels { get; set; }
        public string[] ENV { get; set; }
        public string[] ExposedPorts { get; set; }
        public string[] CMD { get; set; }
    }
}
