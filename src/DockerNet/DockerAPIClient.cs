using DockerNet.Common;
using DockerNet.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DockerNet.Endpoints.Images;
using System.IO.Pipes;
using System.Net.Sockets;

namespace DockerNet
{
    internal delegate void ApiResponseErrorHandlingDelegate(HttpStatusCode statusCode, string responseBody);

    public sealed class DockerAPIClient : IAPIClient
    {
        private const string UserAgent = "DockerNet";

        private static readonly TimeSpan s_InfiniteTimeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
        private readonly HttpClient client;
        private readonly TimeSpan defaultTimeout;
        private readonly Uri endpointBaseUri;
        internal readonly IEnumerable<ApiResponseErrorHandlingDelegate> NoErrorHandlers = Enumerable.Empty<ApiResponseErrorHandlingDelegate>();
        private readonly Version requestedApiVersion;

        public APIConfig Config { get; }
        internal JsonSerializer JsonSerializer { get; private set; }

        public IImagesEndpoint Images { get; }

        private readonly ApiResponseErrorHandlingDelegate _defaultErrorHandlingDelegate = (statusCode, body) =>
        {
            if (statusCode < HttpStatusCode.OK || statusCode >= HttpStatusCode.BadRequest)
            {
                throw new APIException(statusCode, body);
            }
        };

        internal DockerAPIClient(APIConfig config, Version apiVersion = null)
        {
            Config = config;
            requestedApiVersion = apiVersion;
            JsonSerializer = new JsonSerializer();

            Images = new ImagesEndpoint(this);

            ManagedHandler handler;
            var uri = Config.Endpoint;
            switch (uri.Scheme.ToLowerInvariant())
            {
                case "npipe":
                    if (Config.Credentials.IsTlsEnabled())
                    {
                        throw new Exception("TLS not supported over npipe");
                    }

                    var segments = uri.Segments;
                    if (segments.Length != 3 || !segments[1].Equals("pipe/", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException($"{Config.Endpoint} is not a valid npipe URI");
                    }

                    var serverName = uri.Host;
                    var pipeName = uri.Segments[2];

                    uri = new UriBuilder("http", pipeName).Uri;
                    handler = new ManagedHandler(async (host, port, cancellationToken) =>
                    {
                        // NamedPipeClientStream handles file not found by polling until the server arrives. Use a short
                        // timeout so that the user doesn't get stuck waiting for a dockerd instance that is not running.
                        var timeout = 100; // 100ms
                        var stream = new NamedPipeClientStream(serverName, pipeName);
                        var dockerStream = new DockerPipeStream(stream);

                        await stream.ConnectAsync(timeout, cancellationToken);
                        return dockerStream;
                    });

                    break;

                case "tcp":
                case "http":
                    var builder = new UriBuilder(uri)
                    {
                        Scheme = config.Credentials.IsTlsEnabled() ? "https" : "http"
                    };
                    uri = builder.Uri;
                    handler = new ManagedHandler();
                    break;

                case "https":
                    handler = new ManagedHandler();
                    break;

                case "unix":
                    var pipeString = uri.LocalPath;
                    handler = new ManagedHandler(async (string host, int port, CancellationToken cancellationToken) =>
                    {
                        var sock = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                        await sock.ConnectAsync(new UnixDomainSocketEndPoint(pipeString));
                        return sock;
                    });
                    uri = new UriBuilder("http", uri.Segments.Last()).Uri;
                    break;

                default:
                    throw new Exception($"Unknown URL scheme {config.Endpoint.Scheme}");
            }

            endpointBaseUri = uri;

            client = new HttpClient(Config.Credentials.GetHandler(handler), true);
            defaultTimeout = client.Timeout;
            client.Timeout = s_InfiniteTimeout;
        }

        #region Convenience methods

        internal Task<APIResponse> MakeRequestAsync(IEnumerable<ApiResponseErrorHandlingDelegate> errorHandlers, HttpMethod method, string path, string queryString)
        {
            return MakeRequestAsync(errorHandlers, method, path, queryString, null, null, CancellationToken.None);
        }

        internal Task<APIResponse> MakeRequestAsync(IEnumerable<ApiResponseErrorHandlingDelegate> errorHandlers, HttpMethod method, string path, string queryString, IRequestContent data)
        {
            return MakeRequestAsync(errorHandlers, method, path, queryString, data, null, CancellationToken.None);
        }

        internal Task<Stream> MakeRequestForStreamAsync(IEnumerable<ApiResponseErrorHandlingDelegate> errorHandlers, HttpMethod method, string path, string queryString, IRequestContent data, CancellationToken cancellationToken)
        {
            return MakeRequestForStreamAsync(errorHandlers, method, path, queryString, null, data, cancellationToken);
        }

        internal Task<APIResponseStream> MakeRequestForStreamedResponseAsync(IEnumerable<ApiResponseErrorHandlingDelegate> errorHandlers, HttpMethod method, string path, string queryString, IRequestContent data, CancellationToken cancellationToken)
        {
            return MakeRequestForStreamedResponseAsync(errorHandlers, method, path, queryString, null, data, cancellationToken);
        }

        #endregion

        #region HTTP Calls

        internal async Task<APIResponse> MakeRequestAsync(IEnumerable<ApiResponseErrorHandlingDelegate> errorHandlers, HttpMethod method, string path, string queryString, IRequestContent data, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            var response = await MakeRequestInnerAsync(null, HttpCompletionOption.ResponseContentRead, method, path, queryString, null, data, cancellationToken).ConfigureAwait(false);

            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            HandleIfErrorResponse(response.StatusCode, body, errorHandlers);

            return new APIResponse(response.StatusCode, body);
        }

        internal async Task<Stream> MakeRequestForStreamAsync(IEnumerable<ApiResponseErrorHandlingDelegate> errorHandlers, HttpMethod method, string path, string queryString, IDictionary<string, string> headers, IRequestContent data, CancellationToken cancellationToken)
        {
            var response = await MakeRequestInnerAsync(s_InfiniteTimeout, HttpCompletionOption.ResponseHeadersRead, method, path, queryString, headers, data, cancellationToken).ConfigureAwait(false);

            HandleIfErrorResponse(response.StatusCode, null, errorHandlers);

            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        internal async Task<APIResponseStream> MakeRequestForStreamedResponseAsync(IEnumerable<ApiResponseErrorHandlingDelegate> errorHandlers, HttpMethod method, string path, string queryString, IDictionary<string, string> headers, IRequestContent data, CancellationToken cancellationToken)
        {
            var response = await MakeRequestInnerAsync(s_InfiniteTimeout, HttpCompletionOption.ResponseHeadersRead, method, path, queryString, headers, data, cancellationToken);
            HandleIfErrorResponse(response.StatusCode, null, errorHandlers);

            var body = await response.Content.ReadAsStreamAsync();

            return new APIResponseStream(response.StatusCode, body, response.Headers);
        }

        internal async Task<WriteClosableStream> MakeRequestForHijackedStreamAsync(IEnumerable<ApiResponseErrorHandlingDelegate> errorHandlers, HttpMethod method, string path, string queryString, IDictionary<string, string> headers, IRequestContent data, CancellationToken cancellationToken)
        {
            var response = await MakeRequestInnerAsync(s_InfiniteTimeout, HttpCompletionOption.ResponseHeadersRead, method, path, queryString, headers, data, cancellationToken).ConfigureAwait(false);

            HandleIfErrorResponse(response.StatusCode, null, errorHandlers);

            var content = response.Content as HttpConnectionResponseContent;
            if (content == null)
            {
                throw new NotSupportedException("message handler does not support hijacked streams");
            }

            return content.HijackStream();
        }

        private Task<HttpResponseMessage> MakeRequestInnerAsync(TimeSpan? requestTimeout, HttpCompletionOption completionOption, HttpMethod method, string path, string queryString, IDictionary<string, string> headers, IRequestContent data, CancellationToken cancellationToken)
        {
            var request = PrepareRequest(method, path, queryString, headers, data);

            if (requestTimeout.HasValue)
            {
                if (requestTimeout.Value != s_InfiniteTimeout)
                {
                    cancellationToken = CreateTimeoutToken(cancellationToken, requestTimeout.Value);
                }
            }
            else
            {
                cancellationToken = CreateTimeoutToken(cancellationToken, defaultTimeout);
            }

            return client.SendAsync(request, completionOption, cancellationToken);
        }

        private CancellationToken CreateTimeoutToken(CancellationToken token, TimeSpan timeout)
        {
            var timeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

            timeoutTokenSource.CancelAfter(timeout);

            return timeoutTokenSource.Token;
        }

        #endregion

        #region Error handling chain

        private void HandleIfErrorResponse(HttpStatusCode statusCode, string responseBody, IEnumerable<ApiResponseErrorHandlingDelegate> handlers)
        {
            if (handlers == null)
            {
                throw new ArgumentNullException(nameof(handlers));
            }

            foreach (var handler in handlers)
            {
                handler(statusCode, responseBody);
            }

            _defaultErrorHandlingDelegate(statusCode, responseBody);
        }

        #endregion

        internal HttpRequestMessage PrepareRequest(HttpMethod method, string path, string queryString, IDictionary<string, string> headers, IRequestContent data)
        {
            if (string.IsNullOrEmpty("path"))
            {
                throw new ArgumentNullException(nameof(path));
            }

            var request = new HttpRequestMessage(method, RequestBuilder.Build(endpointBaseUri, requestedApiVersion, path, queryString));

            request.Headers.Add("User-Agent", UserAgent);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (data != null)
            {
                var requestContent = data.GetContent(); // make the call only once.
                request.Content = requestContent;
            }

            return request;
        }

        public void Dispose()
        {
            Config.Dispose();
        }
    }
}
