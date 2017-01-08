namespace DockerNet.Endpoints.Images.Entities
{
    public struct BuildImageProgress
    {
        public bool Error { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string Message { get; set; }
    }
}
