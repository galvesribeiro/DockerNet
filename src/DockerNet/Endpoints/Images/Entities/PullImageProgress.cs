namespace DockerNet.Endpoints.Images.Entities
{
    public struct PullImageProgress
    {
        public string Status { get; set; }
        public int Current { get; set; }
        public int Total { get; set; }
    }
}
