using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DockerNet.Endpoints.Images.Entities;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace DockerNet.Endpoints.Images
{
    internal class ImagesEndpoint : IImagesEndpoint
    {
        private const string RegistryAuthHeaderKey = "X-Registry-Auth";
        private readonly DockerAPIClient client;

        internal ImagesEndpoint(DockerAPIClient client)
        {
            this.client = client;
        }

        public Task BuildImage(Dockerfile dockerFile, IProgress<BuildImageProgress> progress, RepositoryAuthInfo authInfo = null)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<Image>> ListImages(ListImagesQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var response = await client
                .MakeRequestAsync(client.NoErrorHandlers, HttpMethod.Get,
                "images/json", query.ToQueryString()).ConfigureAwait(false);

            return client.JsonSerializer.DeserializeObject<Image[]>(response.Body);
        }

        public async Task<ImageDetails> Inspect(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var response = await this.client.MakeRequestAsync(new[] { NoSuchImageHandler }, HttpMethod.Get, $"images/{name}/json", null).ConfigureAwait(false);
            return client.JsonSerializer.DeserializeObject<ImageDetails>(response.Body);
        }

        public async Task<ImageHistory> History(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var response = await this.client.MakeRequestAsync(new[] { NoSuchImageHandler }, HttpMethod.Get, $"images/{name}/history", null).ConfigureAwait(false);
            return client.JsonSerializer.DeserializeObject<ImageHistory>(response.Body);
        }

        public async Task Pull(string image, IProgress<PullImageProgress> progress, string tag = "", RepositoryAuthInfo authInfo = null)
        {
            if (string.IsNullOrWhiteSpace(image)) throw new ArgumentNullException(nameof(image));

            var query = $"fromImage={image}";

            if (!string.IsNullOrWhiteSpace(tag))
                query += $"&tag={tag}";

            var response = await client.MakeRequestForStreamedResponseAsync(
                client.NoErrorHandlers, HttpMethod.Post, "images/create", query, RegistryAuthHeaders(authInfo),
                null, CancellationToken.None);

            if (response.StatusCode != HttpStatusCode.OK) throw new InvalidOperationException("Unable to pull image");

            var reader = new StreamReader(response.Body);
            var report = new PullImageProgress();

            while (response.Body.CanRead && !reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (progress == null) continue;

                var @event = JObject.Parse(line);
                if (@event == null) continue;

                report.Status = @event["status"]?.Value<string>();

                var progressDetail = @event["progressDetail"];
                if (progressDetail != null && progressDetail.HasValues)
                {
                    if (progressDetail["current"] != null)
                    {
                        report.Current = progressDetail["current"].Value<int>();
                    }
                    if (progressDetail["total"] != null)
                    {
                        report.Total = progressDetail["total"].Value<int>();
                    }
                }
                progress.Report(report);
            }
        }

        public async Task Push(string image, IProgress<PushImageProgress> progress, string tag = "", RepositoryAuthInfo authInfo = null)
        {
            if (string.IsNullOrWhiteSpace(image)) throw new ArgumentNullException(nameof(image));

            if (!string.IsNullOrWhiteSpace(tag))
                tag += $"/{tag}";

            var response = await client.MakeRequestForStreamedResponseAsync(
                client.NoErrorHandlers, HttpMethod.Post, $"images/{image.ToLower().Trim()}{tag.ToLower().Trim()}/push", "", RegistryAuthHeaders(authInfo),
                null, CancellationToken.None);

            if (response.StatusCode != HttpStatusCode.OK) throw new InvalidOperationException("Unable to pull image");

            var reader = new StreamReader(response.Body);
            var report = new PushImageProgress();

            while (response.Body.CanRead && !reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (progress == null) continue;

                var @event = JObject.Parse(line);
                if (@event == null) continue;

                report.Status = @event["status"]?.Value<string>();

                var progressDetail = @event["progressDetail"];
                if (progressDetail != null && progressDetail.HasValues)
                {
                    if (progressDetail["current"] != null)
                    {
                        report.Current = progressDetail["current"].Value<int>();
                    }
                    if (progressDetail["total"] != null)
                    {
                        report.Total = progressDetail["total"].Value<int>();
                    }
                }
                progress.Report(report);
            }
        }

        public Task Tag(string sourceImage, string targetImage, string targetTag)
        {
            if (string.IsNullOrWhiteSpace(sourceImage)) throw new ArgumentNullException(nameof(sourceImage));
            if (string.IsNullOrWhiteSpace(targetImage)) throw new ArgumentNullException(nameof(targetImage));
            if (string.IsNullOrWhiteSpace(targetTag)) throw new ArgumentNullException(nameof(targetTag));

            return client.MakeRequestAsync(new[] { NoSuchImageHandler }, HttpMethod.Post, $"images/{sourceImage.ToLower().Trim()}/tag", $"repo={targetImage.ToLower().Trim()}&tag={targetTag.ToLower().Trim()}");
        }

        public async Task<IList<IDictionary<string, string>>> Delete(string image, bool force = false, bool noPrune = false)
        {
            if (string.IsNullOrWhiteSpace(image)) throw new ArgumentNullException(nameof(image));

            var queryString = string.Empty;

            if (force)
                queryString += $"force={force}&";

            if (noPrune)
                queryString += $"noprune={noPrune}&";

            if (queryString.EndsWith("&"))
                queryString = queryString.Remove(queryString.Length - 1);

            var response = await client.MakeRequestAsync(new[] { NoSuchImageHandler }, HttpMethod.Delete, $"images/{image}", queryString).ConfigureAwait(false);
            return client.JsonSerializer.DeserializeObject<Dictionary<string, string>[]>(response.Body);
        }

        public async Task<IList<ImageOverview>> Search(SearchImageQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var response = await client.MakeRequestAsync(client.NoErrorHandlers, HttpMethod.Get, "images/search", query.ToQueryString()).ConfigureAwait(false);
            return client.JsonSerializer.DeserializeObject<ImageOverview[]>(response.Body);
        }

        private Dictionary<string, string> RegistryAuthHeaders(RepositoryAuthInfo authConfig)
        {
            return new Dictionary<string, string>
            {
                {
                    RegistryAuthHeaderKey,
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(this.client.JsonSerializer.SerializeObject(authConfig ?? new RepositoryAuthInfo())))
                }
            };
        }

        internal static readonly ApiResponseErrorHandlingDelegate NoSuchImageHandler = (statusCode, responseBody) =>
        {
            if (statusCode == HttpStatusCode.NotFound)
            {
                throw new ImageNotFoundException(statusCode, responseBody);
            }
        };
    }
}
