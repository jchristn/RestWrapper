namespace Test.Shared
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using WatsonWebserver;
    using WatsonWebserver.Core;

    /// <summary>
    /// Local HTTP server used by the automated RestWrapper test suite.
    /// </summary>
    public class RestWrapperTestServer : IDisposable, IAsyncDisposable
    {
        private readonly string _Hostname = "127.0.0.1";
        private readonly int _Port;
        private readonly Webserver _Webserver;
        private bool _Disposed = false;

        /// <summary>
        /// Base URL for the running server.
        /// </summary>
        public string BaseUrl
        {
            get
            {
                return "http://" + _Hostname + ":" + _Port.ToString();
            }
        }

        private RestWrapperTestServer(int port)
        {
            _Port = port;
            _Webserver = new Webserver(new WebserverSettings(_Hostname, _Port, false), RouteAsync);
        }

        /// <summary>
        /// Start a new local test server on a free loopback port.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Running test server.</returns>
        public static async Task<RestWrapperTestServer> StartAsync(CancellationToken cancellationToken = default)
        {
            int port = GetAvailablePort();
            RestWrapperTestServer server = new RestWrapperTestServer(port);
            server._Webserver.Start();
            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            return server;
        }

        /// <summary>
        /// Build an absolute URL for a server-relative path.
        /// </summary>
        /// <param name="path">Relative path.</param>
        /// <returns>Absolute URL.</returns>
        public string GetUrl(string path)
        {
            if (String.IsNullOrEmpty(path)) path = "/";
            if (!path.StartsWith("/")) path = "/" + path;
            return BaseUrl + path;
        }

        /// <summary>
        /// Create a new HttpClient for talking to the server.
        /// </summary>
        /// <param name="timeout">Optional timeout override.</param>
        /// <returns>HTTP client instance.</returns>
        public System.Net.Http.HttpClient CreateHttpClient(TimeSpan? timeout = null)
        {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            if (timeout != null) client.Timeout = timeout.Value;
            return client;
        }

        /// <summary>
        /// Dispose the server.
        /// </summary>
        public void Dispose()
        {
            if (_Disposed) return;
            _Webserver.Stop();
            _Webserver.Dispose();
            _Disposed = true;
        }

        /// <summary>
        /// Dispose the server asynchronously.
        /// </summary>
        /// <returns>Completed value task.</returns>
        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }

        private static int GetAvailablePort()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private async Task RouteAsync(HttpContextBase context)
        {
            string path = context.Request.Url.RawWithoutQuery;

            try
            {
                switch (path)
                {
                    case "/test":
                        await SendTextAsync(context, 200, "Hello from the local RestWrapper test server.").ConfigureAwait(false);
                        break;
                    case "/text":
                        await SendTextAsync(context, 200, "Sample text response for testing").ConfigureAwait(false);
                        break;
                    case "/echo":
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = String.IsNullOrEmpty(context.Request.ContentType)
                            ? "text/plain"
                            : context.Request.ContentType;
                        ApplyStandardHeaders(context);
                        await context.Response.Send(context.Request.DataAsString ?? String.Empty).ConfigureAwait(false);
                        break;
                    case "/methods":
                    case "/inspect":
                    case "/chunked-inspect":
                        await SendJsonAsync(context, 200, BuildSnapshot(context, path)).ConfigureAwait(false);
                        break;
                    case "/chunked":
                        await SendChunkedAsync(context, 5, 25, "Server chunk ").ConfigureAwait(false);
                        break;
                    case "/chunked-slow":
                        await SendChunkedAsync(context, 3, 400, "Slow chunk ").ConfigureAwait(false);
                        break;
                    case "/sse":
                        await SendServerSentEventsAsync(context).ConfigureAwait(false);
                        break;
                    case "/sse-complex":
                        await SendRawAsync(
                            context,
                            200,
                            "text/event-stream",
                            ": keep-alive\nid: 42\nevent: update\nretry: 1500\ndata: line 1\ndata: line 2\n\n:event comment\n\nevent: heartbeat\n\nid: 99\ndata: final payload")
                            .ConfigureAwait(false);
                        break;
                    case "/delay":
                        await Task.Delay(1000).ConfigureAwait(false);
                        await SendTextAsync(context, 200, "Delayed response").ConfigureAwait(false);
                        break;
                    case "/redirect-text":
                        await SendRedirectAsync(context, "/text").ConfigureAwait(false);
                        break;
                    default:
                        await SendTextAsync(context, 404, "Not found").ConfigureAwait(false);
                        break;
                }
            }
            catch (Exception e)
            {
                await SendTextAsync(context, 500, "Server error: " + e.Message).ConfigureAwait(false);
            }
        }

        private static RequestSnapshot BuildSnapshot(HttpContextBase context, string path)
        {
            AuthorizationDetails? authorization = context.Request.Authorization;

            return new RequestSnapshot
            {
                Method = context.Request.Method.ToString(),
                Path = path,
                Body = context.Request.DataAsString ?? String.Empty,
                ContentType = context.Request.ContentType ?? String.Empty,
                TransferEncoding = context.Request.Headers.Get("Transfer-Encoding") ?? String.Empty,
                ContentLanguage = context.Request.Headers.Get("Content-Language") ?? String.Empty,
                Accept = context.Request.Headers.Get("Accept") ?? String.Empty,
                Host = context.Request.Headers.Get("Host") ?? String.Empty,
                Authorization = authorization?.Value ?? String.Empty,
                AuthorizationUsername = authorization?.Username ?? String.Empty,
                AuthorizationPassword = authorization?.Password ?? String.Empty,
                AuthorizationBearerToken = authorization?.BearerToken ?? String.Empty,
                CustomHeader = context.Request.Headers.Get("X-Custom-Header") ?? String.Empty,
                AnotherHeader = context.Request.Headers.Get("X-Another-Header") ?? String.Empty,
                UserAgent = context.Request.Headers.Get("User-Agent") ?? String.Empty
            };
        }

        private static async Task SendJsonAsync(HttpContextBase context, int statusCode, object payload)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            ApplyStandardHeaders(context);
            string json = JsonSerializer.Serialize(payload);
            await context.Response.Send(json).ConfigureAwait(false);
        }

        private static async Task SendTextAsync(HttpContextBase context, int statusCode, string payload)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "text/plain";
            ApplyStandardHeaders(context);
            await context.Response.Send(payload).ConfigureAwait(false);
        }

        private static async Task SendRawAsync(HttpContextBase context, int statusCode, string contentType, string payload)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = contentType;
            ApplyStandardHeaders(context);
            await context.Response.Send(payload).ConfigureAwait(false);
        }

        private static async Task SendRedirectAsync(HttpContextBase context, string location)
        {
            context.Response.StatusCode = 302;
            context.Response.Headers.Add("Location", location);
            ApplyStandardHeaders(context);
            await context.Response.Send(String.Empty).ConfigureAwait(false);
        }

        private static async Task SendChunkedAsync(HttpContextBase context, int chunkCount, int delayMilliseconds, string prefix)
        {
            context.Response.ChunkedTransfer = true;
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/plain";
            ApplyStandardHeaders(context);

            for (int i = 0; i < chunkCount; i++)
            {
                byte[] data = Encoding.UTF8.GetBytes(prefix + i.ToString() + "\n");
                await context.Response.SendChunk(data, false).ConfigureAwait(false);
                await Task.Delay(delayMilliseconds).ConfigureAwait(false);
            }

            await context.Response.SendChunk(Array.Empty<byte>(), true).ConfigureAwait(false);
        }

        private static async Task SendServerSentEventsAsync(HttpContextBase context)
        {
            context.Response.StatusCode = 200;
            context.Response.ServerSentEvents = true;
            ApplyStandardHeaders(context);

            for (int i = 0; i < 5; i++)
            {
                await context.Response.SendEvent(new ServerSentEvent
                {
                    Id = i.ToString(),
                    Data = "Event data " + i.ToString()
                }, false).ConfigureAwait(false);

                await Task.Delay(25).ConfigureAwait(false);
            }

            await context.Response.SendEvent(new ServerSentEvent
            {
                Data = "Final event"
            }, true).ConfigureAwait(false);
        }

        private static void ApplyStandardHeaders(HttpContextBase context)
        {
            context.Response.Headers.Add("X-Test-Server", "RestWrapperTestServer");
        }
    }
}
