using DockerNet;
using DockerNet.Common;
using DockerNet.Endpoints.Images;
using DockerNet.Endpoints.Images.Entities;
using Newtonsoft.Json;
using System;

class Program
{
    static void Main(string[] args)
    {
        var query = new ListImagesQuery();
        query.WithDangling().WithLabel("aaaa").WithLabel("bbbbb", LabelOperators.EqualTo, "123").GetAll();

        var config = new APIConfig(new Uri("http://localhost:2375"));
        var client = new DockerAPIClient(config);
        
        var images = client.Images.Search(new SearchImageQuery { Term = "docker" }).Result;
        Console.Write("Done!");
        Console.WriteLine();
    }
}

public class Reporter<T> : IProgress<T>
{
    public void Report(T value)
    {
        Console.WriteLine(JsonConvert.SerializeObject(value));
    }
}