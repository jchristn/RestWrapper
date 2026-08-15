namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using RestWrapper;

    using Touchstone.Core;

    /// <summary>
    /// Shared Touchstone test suites covering RestWrapper behavior with both internal and caller-supplied HttpClient instances.
    /// </summary>
    public static partial class RestWrapperSuites
    {
        /// <summary>
        /// All automated test suites.
        /// </summary>
        public static IReadOnlyList<TestSuiteDescriptor> All
        {
            get
            {
                return new List<TestSuiteDescriptor>
                {
                    ValidationSuite(),
                    HelperSuite(),
                    CoreRequestSuite(RequestTransportMode.Internal),
                    CoreRequestSuite(RequestTransportMode.External),
                    StreamingSuite(RequestTransportMode.Internal),
                    StreamingSuite(RequestTransportMode.External),
                    TransportExpansionSuite(RequestTransportMode.Internal),
                    TransportExpansionSuite(RequestTransportMode.External),
                    ResponseSuite(RequestTransportMode.Internal),
                    ResponseSuite(RequestTransportMode.External),
                    DirectResponseSuite(),
                    ServerSentEventReaderSuite(),
                    ExternalClientSuite(),
                    InternalClientSuite(),
                    InternalTransportFeatureSuite(),
                    ExternalTransportFeatureSuite()
                };
            }
        }

        private static TestSuiteDescriptor ValidationSuite()
        {
            string suiteId = "Validation";

            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Validation and property guards",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "NullUrlThrows", "Constructors reject null URLs", ct => TestNullUrlThrowsAsync()),
                    new TestCaseDescriptor(suiteId, "NullHttpClientThrows", "Caller-supplied HttpClient constructors reject null clients", ct => TestNullHttpClientThrowsAsync()),
                    new TestCaseDescriptor(suiteId, "BufferSizeRejectsZero", "BufferSize rejects zero", ct => TestBufferSizeRejectsZeroAsync()),
                    new TestCaseDescriptor(suiteId, "BufferSizeRejectsNegative", "BufferSize rejects negative values", ct => TestBufferSizeRejectsNegativeAsync()),
                    new TestCaseDescriptor(suiteId, "BufferSizeAcceptsPositive", "BufferSize accepts and round-trips positive values", ct => TestBufferSizeAcceptsPositiveAsync()),
                    new TestCaseDescriptor(suiteId, "TimeoutRejectsZero", "TimeoutMilliseconds rejects zero", ct => TestTimeoutRejectsZeroAsync()),
                    new TestCaseDescriptor(suiteId, "TimeoutRejectsNegative", "TimeoutMilliseconds rejects negative values", ct => TestTimeoutRejectsNegativeAsync()),
                    new TestCaseDescriptor(suiteId, "TimeoutAcceptsPositive", "TimeoutMilliseconds accepts and round-trips positive values", ct => TestTimeoutAcceptsPositiveAsync()),
                    new TestCaseDescriptor(suiteId, "ContentLengthRejectsNegative", "ContentLength rejects negative values", ct => TestContentLengthRejectsNegativeAsync()),
                    new TestCaseDescriptor(suiteId, "ContentLengthAcceptsZeroAndPositive", "ContentLength accepts zero and positive values", ct => TestContentLengthAcceptsZeroAndPositiveAsync()),
                    new TestCaseDescriptor(suiteId, "DefaultMethodIsGet", "RestRequest defaults to the GET method", ct => TestDefaultMethodIsGetAsync()),
                    new TestCaseDescriptor(suiteId, "QueryParsesValues", "Query property parses URL query parameters", ct => TestQueryParsingAsync()),
                    new TestCaseDescriptor(suiteId, "QueryWithoutQueryStringIsEmpty", "Query property is empty when the URL has no query string", ct => TestQueryWithoutQueryStringIsEmptyAsync())
                });
        }

        private static TestSuiteDescriptor CoreRequestSuite(RequestTransportMode mode)
        {
            string suiteId = "Core." + ModeId(mode);
            string modeLabel = ModeLabel(mode);

            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Core request behavior (" + modeLabel + ")",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "HeadReturnsHeaders", "HEAD returns headers using " + modeLabel, ct => TestHeadAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "GetReturnsBody", "GET returns body and path using " + modeLabel, ct => TestGetAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "PostStringBody", "POST string payload round-trips using " + modeLabel, ct => TestPostStringAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "PutStreamBody", "PUT stream payload round-trips using " + modeLabel, ct => TestPutStreamAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "PatchByteArrayBody", "PATCH byte[] payload round-trips using " + modeLabel, ct => TestPatchByteArrayAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "DeleteHandled", "DELETE is routed correctly using " + modeLabel, ct => TestDeleteAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "OptionsHandled", "OPTIONS is routed correctly using " + modeLabel, ct => TestOptionsAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "FormEncoded", "Form payloads round-trip using " + modeLabel, ct => TestFormEncodedAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "HeadersAndUserAgent", "Custom headers and User-Agent are per-request using " + modeLabel, ct => TestHeadersAndUserAgentAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "BasicAuthorization", "Basic authorization works using " + modeLabel, ct => TestBasicAuthorizationAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "BearerAuthorization", "Bearer authorization works using " + modeLabel, ct => TestBearerAuthorizationAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "RawAuthorization", "Raw authorization works using " + modeLabel, ct => TestRawAuthorizationAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "JsonSerialization", "JSON payloads deserialize correctly using " + modeLabel, ct => TestJsonSerializationAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "CustomSerializer", "Custom serializers work using " + modeLabel, ct => TestCustomSerializerAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "NotFound", "404 responses propagate using " + modeLabel, ct => TestNotFoundAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "InvalidUrlThrows", "Invalid URLs fail cleanly using " + modeLabel, ct => TestInvalidUrlAsync(mode, ct))
                });
        }

        private static TestSuiteDescriptor StreamingSuite(RequestTransportMode mode)
        {
            string suiteId = "Streaming." + ModeId(mode);
            string modeLabel = ModeLabel(mode);

            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Streaming and cancellation (" + modeLabel + ")",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "ChunkedTransfer", "Chunked transfer works using " + modeLabel, ct => TestChunkedTransferAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "ServerSentEvents", "Server-sent events work using " + modeLabel, ct => TestServerSentEventsAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "RequestCancellation", "Request cancellation is honored using " + modeLabel, ct => TestRequestCancellationAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "ChunkReadCancellation", "Chunk-read cancellation is honored using " + modeLabel, ct => TestChunkReadCancellationAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "TimeoutPath", mode == RequestTransportMode.Internal
                        ? "RestRequest timeout is honored with an internally-owned client"
                        : "Caller-owned HttpClient timeout is honored",
                        ct => mode == RequestTransportMode.Internal
                            ? TestInternalTimeoutAsync(ct)
                            : TestExternalHttpClientTimeoutAsync(ct))
                });
        }

        private static TestSuiteDescriptor ResponseSuite(RequestTransportMode mode)
        {
            string suiteId = "Response." + ModeId(mode);
            string modeLabel = ModeLabel(mode);

            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Response data handling (" + modeLabel + ")",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "DataAsStringStable", "DataAsString is stable using " + modeLabel, ct => TestDataAsStringStableAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "DataAsBytesMatch", "DataAsBytes matches DataAsString using " + modeLabel, ct => TestDataAsBytesMatchAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "DataStreamReadable", "Data stream is readable using " + modeLabel, ct => TestDataStreamReadableAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "RegularPropertiesAccessible", "Regular responses expose normal properties using " + modeLabel, ct => TestRegularResponsePropertiesAsync(mode, ct))
                });
        }

        private static TestSuiteDescriptor ExternalClientSuite()
        {
            string suiteId = "ExternalClient";

            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Caller-supplied HttpClient semantics",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "ExternalClientNotDisposed", "RestRequest does not dispose a caller-supplied HttpClient", ct => TestExternalClientNotDisposedAsync(ct)),
                    new TestCaseDescriptor(suiteId, "SharedClientStateIsolation", "Shared caller-supplied clients do not leak per-request headers or auth", ct => TestSharedClientStateIsolationAsync(ct)),
                    new TestCaseDescriptor(suiteId, "TimeoutConflictThrows", "TimeoutMilliseconds conflicts with caller-supplied clients", ct => TestExternalTimeoutConflictAsync(ct)),
                    new TestCaseDescriptor(suiteId, "AutoRedirectConflictThrows", "AllowAutoRedirect conflicts with caller-supplied clients", ct => TestExternalAutoRedirectConflictAsync(ct)),
                    new TestCaseDescriptor(suiteId, "IgnoreCertificateConflictThrows", "IgnoreCertificateErrors conflicts with caller-supplied clients", ct => TestExternalIgnoreCertificateConflictAsync(ct)),
                    new TestCaseDescriptor(suiteId, "CertificateFilenameConflictThrows", "CertificateFilename conflicts with caller-supplied clients", ct => TestExternalCertificateFilenameConflictAsync(ct)),
                    new TestCaseDescriptor(suiteId, "CertificatePasswordConflictThrows", "CertificatePassword conflicts with caller-supplied clients", ct => TestExternalCertificatePasswordConflictAsync(ct))
                });
        }

        private static TestSuiteDescriptor InternalClientSuite()
        {
            string suiteId = "InternalClient";

            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Internally-owned HttpClient semantics",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "TimeoutPropertyCancelsRequest", "TimeoutMilliseconds cancels requests for internally-owned clients", ct => TestInternalTimeoutAsync(ct))
                });
        }

        private static Task TestNullUrlThrowsAsync()
        {
            AssertThrows<ArgumentNullException>(() => new RestRequest(null!));
            AssertThrows<ArgumentNullException>(() => new RestRequest(null!, HttpMethod.Get));
            return Task.CompletedTask;
        }

        private static Task TestNullHttpClientThrowsAsync()
        {
            AssertThrows<ArgumentNullException>(() => new RestRequest("http://127.0.0.1", (HttpClient)null!));
            AssertThrows<ArgumentNullException>(() => new RestRequest("http://127.0.0.1", HttpMethod.Get, (HttpClient)null!));
            return Task.CompletedTask;
        }

        private static Task TestBufferSizeRejectsZeroAsync()
        {
            using RestRequest request = new RestRequest("http://127.0.0.1");
            AssertThrows<ArgumentException>(() => request.BufferSize = 0);
            return Task.CompletedTask;
        }

        private static Task TestBufferSizeRejectsNegativeAsync()
        {
            using RestRequest request = new RestRequest("http://127.0.0.1");
            AssertThrows<ArgumentException>(() => request.BufferSize = -1);
            return Task.CompletedTask;
        }

        private static Task TestBufferSizeAcceptsPositiveAsync()
        {
            using RestRequest request = new RestRequest("http://127.0.0.1");
            request.BufferSize = 1;
            AssertEqual(1, request.BufferSize, "BufferSize (minimum)");
            request.BufferSize = 16384;
            AssertEqual(16384, request.BufferSize, "BufferSize");
            return Task.CompletedTask;
        }

        private static Task TestTimeoutRejectsZeroAsync()
        {
            using RestRequest request = new RestRequest("http://127.0.0.1");
            AssertThrows<ArgumentException>(() => request.TimeoutMilliseconds = 0);
            return Task.CompletedTask;
        }

        private static Task TestTimeoutRejectsNegativeAsync()
        {
            using RestRequest request = new RestRequest("http://127.0.0.1");
            AssertThrows<ArgumentException>(() => request.TimeoutMilliseconds = -1);
            return Task.CompletedTask;
        }

        private static Task TestTimeoutAcceptsPositiveAsync()
        {
            using RestRequest request = new RestRequest("http://127.0.0.1");
            request.TimeoutMilliseconds = 1;
            AssertEqual(1, request.TimeoutMilliseconds, "TimeoutMilliseconds (minimum)");
            request.TimeoutMilliseconds = 30000;
            AssertEqual(30000, request.TimeoutMilliseconds, "TimeoutMilliseconds");
            return Task.CompletedTask;
        }

        private static Task TestContentLengthRejectsNegativeAsync()
        {
            using RestRequest request = new RestRequest("http://127.0.0.1");
            AssertThrows<ArgumentOutOfRangeException>(() => request.ContentLength = -1);
            return Task.CompletedTask;
        }

        private static Task TestContentLengthAcceptsZeroAndPositiveAsync()
        {
            using RestRequest request = new RestRequest("http://127.0.0.1");
            request.ContentLength = 0;
            AssertEqual(0, (int)request.ContentLength, "ContentLength (zero)");
            request.ContentLength = 4096;
            AssertEqual(4096, (int)request.ContentLength, "ContentLength");
            return Task.CompletedTask;
        }

        private static Task TestDefaultMethodIsGetAsync()
        {
            using RestRequest request = new RestRequest("http://127.0.0.1");
            AssertEqual("GET", request.Method.Method, "default method");
            return Task.CompletedTask;
        }

        private static Task TestQueryParsingAsync()
        {
            using RestRequest request = new RestRequest("http://127.0.0.1/test?param1=value1&param2=value2&flag");

            AssertEqual("value1", request.Query["param1"] ?? string.Empty, "param1");
            AssertEqual("value2", request.Query["param2"] ?? string.Empty, "param2");
            AssertTrue(request.Query.AllKeys != null && Array.Exists(request.Query.AllKeys, x => x == "flag"), "Expected flag query parameter");
            AssertEqual(string.Empty, request.Query["flag"] ?? string.Empty, "flag");

            return Task.CompletedTask;
        }

        private static Task TestQueryWithoutQueryStringIsEmptyAsync()
        {
            using RestRequest request = new RestRequest("http://127.0.0.1/test");

            AssertNotNull(request.Query, "Query");
            AssertEqual(0, request.Query.Count, "Query.Count");

            return Task.CompletedTask;
        }

        private static async Task TestHeadAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/methods", HttpMethod.Head, async (server, client, request, ct) =>
            {
                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);
                AssertTrue(response.Headers != null, "HEAD response should expose headers");
                AssertTrue(response.Data == null, "HEAD response should not expose a body stream");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestGetAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/methods", HttpMethod.Get, async (server, client, request, ct) =>
            {
                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);

                RequestSnapshot snapshot = DeserializeJson<RequestSnapshot>(response);
                AssertEqual("GET", snapshot.Method, "method");
                AssertEqual("/methods", snapshot.Path, "path");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestPostStringAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/echo", HttpMethod.Post, async (server, client, request, ct) =>
            {
                request.ContentType = "text/plain";
                string payload = "This is test data to echo back";

                using RestResponse response = await request.SendAsync(payload, ct).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);
                AssertEqual(payload, response.DataAsString, "echo payload");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestPutStreamAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/methods", HttpMethod.Put, async (server, client, request, ct) =>
            {
                request.ContentType = "text/plain";
                byte[] payloadBytes = Encoding.UTF8.GetBytes("stream payload");
                using MemoryStream stream = new MemoryStream(payloadBytes);

                using RestResponse response = await request.SendAsync(payloadBytes.Length, stream, ct).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);

                RequestSnapshot snapshot = DeserializeJson<RequestSnapshot>(response);
                AssertEqual("PUT", snapshot.Method, "method");
                AssertEqual("stream payload", snapshot.Body, "body");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestPatchByteArrayAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/methods", new HttpMethod("PATCH"), async (server, client, request, ct) =>
            {
                request.ContentType = "application/json";
                string payload = "{\"patch\":\"operation\",\"field\":\"value\"}";

                using RestResponse response = await request.SendAsync(Encoding.UTF8.GetBytes(payload), ct).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);

                RequestSnapshot snapshot = DeserializeJson<RequestSnapshot>(response);
                AssertEqual("PATCH", snapshot.Method, "method");
                AssertEqual(payload, snapshot.Body, "body");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestDeleteAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/methods", HttpMethod.Delete, async (server, client, request, ct) =>
            {
                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);

                RequestSnapshot snapshot = DeserializeJson<RequestSnapshot>(response);
                AssertEqual("DELETE", snapshot.Method, "method");
                AssertEqual(string.Empty, snapshot.Body, "body");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestOptionsAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/methods", HttpMethod.Options, async (server, client, request, ct) =>
            {
                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);

                RequestSnapshot snapshot = DeserializeJson<RequestSnapshot>(response);
                AssertEqual("OPTIONS", snapshot.Method, "method");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestFormEncodedAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/methods", HttpMethod.Post, async (server, client, request, ct) =>
            {
                Dictionary<string, string> form = new Dictionary<string, string>
                {
                    { "foo", "bar" },
                    { "hello", "world how are you" }
                };

                using RestResponse response = await request.SendAsync(form, ct).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);

                RequestSnapshot snapshot = DeserializeJson<RequestSnapshot>(response);
                AssertEqual("POST", snapshot.Method, "method");
                AssertTrue(snapshot.Body.Contains("foo=bar", StringComparison.Ordinal), "Form body missing foo field");
                AssertTrue(snapshot.Body.Contains("hello=world+how+are+you", StringComparison.Ordinal), "Form body missing hello field");
                AssertTrue(snapshot.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase), "Unexpected form content type");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestHeadersAndUserAgentAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/inspect", HttpMethod.Get, async (server, client, request, ct) =>
            {
                request.Headers.Add("X-Custom-Header", "test-value");
                request.Headers.Add("X-Another-Header", "another-value");
                request.UserAgent = "RestWrapper.TestSuite/1.0";

                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);

                RequestSnapshot snapshot = DeserializeJson<RequestSnapshot>(response);
                AssertEqual("test-value", snapshot.CustomHeader, "X-Custom-Header");
                AssertEqual("another-value", snapshot.AnotherHeader, "X-Another-Header");
                AssertTrue(snapshot.UserAgent.Contains("RestWrapper.TestSuite/1.0", StringComparison.Ordinal), "User-Agent was not propagated");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestBasicAuthorizationAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/inspect", HttpMethod.Get, async (server, client, request, ct) =>
            {
                request.Authorization.User = "alpha";
                request.Authorization.Password = "beta";

                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);

                RequestSnapshot snapshot = DeserializeJson<RequestSnapshot>(response);
                AssertEqual("alpha", snapshot.AuthorizationUsername, "basic username");
                AssertEqual("beta", snapshot.AuthorizationPassword, "basic password");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestBearerAuthorizationAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/inspect", HttpMethod.Get, async (server, client, request, ct) =>
            {
                request.Authorization.BearerToken = "test-bearer-token";

                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);

                RequestSnapshot snapshot = DeserializeJson<RequestSnapshot>(response);
                AssertEqual("test-bearer-token", snapshot.AuthorizationBearerToken, "bearer token");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestRawAuthorizationAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/inspect", HttpMethod.Get, async (server, client, request, ct) =>
            {
                request.Authorization.Raw = "Custom token-value";

                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);

                RequestSnapshot snapshot = DeserializeJson<RequestSnapshot>(response);
                AssertEqual("Custom token-value", snapshot.Authorization, "raw authorization");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestJsonSerializationAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/echo", HttpMethod.Post, async (server, client, request, ct) =>
            {
                SamplePayload payload = new SamplePayload
                {
                    Name = "Test",
                    Value = 42
                };

                request.ContentType = "application/json";
                string json = JsonSerializer.Serialize(payload);

                using RestResponse response = await request.SendAsync(json, ct).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);

                SamplePayload result = response.DataFromJson<SamplePayload>();
                AssertEqual(payload.Name, result.Name, "name");
                AssertEqual(payload.Value, result.Value, "value");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestCustomSerializerAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/echo", HttpMethod.Post, async (server, client, request, ct) =>
            {
                SamplePayload payload = new SamplePayload
                {
                    Name = "Custom",
                    Value = 99
                };

                CustomSerializer serializer = new CustomSerializer();
                request.ContentType = "application/json";
                string json = serializer.SerializeJson(payload, false);

                using RestResponse response = await request.SendAsync(json, ct).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);
                response.SerializationHelper = serializer;

                SamplePayload result = response.DataFromJson<SamplePayload>();
                AssertEqual(payload.Name, result.Name, "name");
                AssertEqual(payload.Value, result.Value, "value");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestNotFoundAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/nonexistent", HttpMethod.Get, async (server, client, request, ct) =>
            {
                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                AssertStatus(404, response.StatusCode);
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestInvalidUrlAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            if (mode == RequestTransportMode.Internal)
            {
                using RestRequest request = new RestRequest("invalid-url");
                await AssertThrowsAsync<Exception>(async () =>
                {
                    using RestResponse response = await request.SendAsync(cancellationToken).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            else
            {
                using HttpClient client = new HttpClient();
                using RestRequest request = new RestRequest("invalid-url", client);
                await AssertThrowsAsync<Exception>(async () =>
                {
                    using RestResponse response = await request.SendAsync(cancellationToken).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        private static async Task TestChunkedTransferAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/chunked", HttpMethod.Post, async (server, client, request, ct) =>
            {
                request.ChunkedTransfer = true;

                await request.SendChunkAsync("Client chunk 0", false, ct).ConfigureAwait(false);
                await request.SendChunkAsync("Client chunk 1", false, ct).ConfigureAwait(false);

                using RestResponse response = await request.SendChunkAsync(Array.Empty<byte>(), true, ct).ConfigureAwait(false);
                AssertTrue(response.ChunkedTransferEncoding, "Expected chunked response");

                List<string> chunks = new List<string>();

                while (true)
                {
                    ChunkData? chunk = await response.ReadChunkAsync(ct).ConfigureAwait(false);
                    if (chunk == null) break;

                    if (chunk.Data != null && chunk.Data.Length > 0)
                        chunks.Add(Encoding.UTF8.GetString(chunk.Data));

                    if (chunk.IsFinal) break;
                }

                AssertEqual(5, chunks.Count, "chunk count");

                for (int i = 0; i < chunks.Count; i++)
                {
                    AssertEqual("Server chunk " + i.ToString(), chunks[i], "chunk " + i.ToString());
                }
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestServerSentEventsAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/sse", HttpMethod.Get, async (server, client, request, ct) =>
            {
                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                AssertTrue(response.ServerSentEvents, "Expected SSE response");

                List<string> events = new List<string>();

                while (true)
                {
                    ServerSentEvent? evt = await response.ReadEventAsync(ct).ConfigureAwait(false);
                    if (evt == null) break;

                    if (!String.IsNullOrEmpty(evt.Data))
                        events.Add(evt.Data.Trim());

                    if (events.Count >= 6) break;
                }

                AssertEqual(6, events.Count, "event count");

                for (int i = 0; i < 5; i++)
                {
                    AssertEqual("Event data " + i.ToString(), events[i], "event " + i.ToString());
                }

                AssertEqual("Final event", events[5], "final event");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestRequestCancellationAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/delay", HttpMethod.Get, async (server, client, request, ct) =>
            {
                using CancellationTokenSource cancellation = new CancellationTokenSource();
                cancellation.CancelAfter(100);

                await AssertThrowsAsync<OperationCanceledException>(async () =>
                {
                    using RestResponse response = await request.SendAsync(cancellation.Token).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestChunkReadCancellationAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/chunked-slow", HttpMethod.Post, async (server, client, request, ct) =>
            {
                request.ChunkedTransfer = true;

                using RestResponse response = await request.SendChunkAsync(Array.Empty<byte>(), true, ct).ConfigureAwait(false);
                ChunkData? firstChunk = await response.ReadChunkAsync(ct).ConfigureAwait(false);
                AssertTrue(firstChunk != null, "Expected at least one chunk");

                using CancellationTokenSource cancellation = new CancellationTokenSource();
                cancellation.CancelAfter(50);

                await AssertThrowsAsync<OperationCanceledException>(async () =>
                {
                    ChunkData? secondChunk = await response.ReadChunkAsync(cancellation.Token).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestExternalHttpClientTimeoutAsync(CancellationToken cancellationToken)
        {
            await using RestWrapperTestServer server = await RestWrapperTestServer.StartAsync(cancellationToken).ConfigureAwait(false);
            using HttpClient client = server.CreateHttpClient(TimeSpan.FromMilliseconds(100));
            using RestRequest request = new RestRequest(server.GetUrl("/delay"), client);

            await AssertThrowsAsync<TaskCanceledException>(async () =>
            {
                using RestResponse response = await request.SendAsync(cancellationToken).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        private static async Task TestInternalTimeoutAsync(CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(RequestTransportMode.Internal, "/delay", HttpMethod.Get, async (server, client, request, ct) =>
            {
                request.TimeoutMilliseconds = 100;

                await AssertThrowsAsync<TaskCanceledException>(async () =>
                {
                    using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestDataAsStringStableAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/text", HttpMethod.Get, async (server, client, request, ct) =>
            {
                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                string first = response.DataAsString;
                string second = response.DataAsString;

                AssertTrue(!String.IsNullOrEmpty(first), "DataAsString should not be empty");
                AssertEqual(first, second, "DataAsString");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestDataAsBytesMatchAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/text", HttpMethod.Get, async (server, client, request, ct) =>
            {
                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                byte[] bytes = response.DataAsBytes;
                string fromBytes = Encoding.UTF8.GetString(bytes);

                AssertTrue(bytes.Length > 0, "DataAsBytes should not be empty");
                AssertEqual(fromBytes, response.DataAsString, "DataAsBytes/DataAsString");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestDataStreamReadableAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/text", HttpMethod.Get, async (server, client, request, ct) =>
            {
                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                Stream? stream = response.Data;

                AssertTrue(stream != null, "Data stream should not be null");
                AssertTrue(stream!.CanRead, "Data stream should be readable");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestRegularResponsePropertiesAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/text", HttpMethod.Get, async (server, client, request, ct) =>
            {
                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                Stream? data = response.Data;
                string dataString = response.DataAsString;
                byte[] dataBytes = response.DataAsBytes;

                AssertTrue(data != null, "Data should be accessible");
                AssertTrue(!String.IsNullOrEmpty(dataString), "DataAsString should be accessible");
                AssertTrue(dataBytes.Length > 0, "DataAsBytes should be accessible");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestExternalClientNotDisposedAsync(CancellationToken cancellationToken)
        {
            await using RestWrapperTestServer server = await RestWrapperTestServer.StartAsync(cancellationToken).ConfigureAwait(false);
            using HttpClient client = server.CreateHttpClient();

            using (RestRequest request = new RestRequest(server.GetUrl("/text"), client))
            {
                using RestResponse response = await request.SendAsync(cancellationToken).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);
            }

            using HttpResponseMessage verification = await client.GetAsync(server.GetUrl("/text"), cancellationToken).ConfigureAwait(false);
            AssertStatus(200, (int)verification.StatusCode);
        }

        private static async Task TestSharedClientStateIsolationAsync(CancellationToken cancellationToken)
        {
            await using RestWrapperTestServer server = await RestWrapperTestServer.StartAsync(cancellationToken).ConfigureAwait(false);
            using HttpClient client = server.CreateHttpClient();

            RequestSnapshot firstSnapshot;
            RequestSnapshot secondSnapshot;
            RequestSnapshot thirdSnapshot;

            using (RestRequest first = new RestRequest(server.GetUrl("/inspect"), HttpMethod.Get, client))
            {
                first.Headers.Add("X-Custom-Header", "first-value");
                first.Authorization.BearerToken = "first-token";

                using RestResponse response = await first.SendAsync(cancellationToken).ConfigureAwait(false);
                firstSnapshot = DeserializeJson<RequestSnapshot>(response);
            }

            using (RestRequest second = new RestRequest(server.GetUrl("/inspect"), HttpMethod.Get, client))
            {
                using RestResponse response = await second.SendAsync(cancellationToken).ConfigureAwait(false);
                secondSnapshot = DeserializeJson<RequestSnapshot>(response);
            }

            using (RestRequest third = new RestRequest(server.GetUrl("/inspect"), HttpMethod.Get, client))
            {
                third.Headers.Add("X-Custom-Header", "second-value");
                third.Authorization.BearerToken = "second-token";

                using RestResponse response = await third.SendAsync(cancellationToken).ConfigureAwait(false);
                thirdSnapshot = DeserializeJson<RequestSnapshot>(response);
            }

            AssertEqual("first-value", firstSnapshot.CustomHeader, "first custom header");
            AssertEqual("first-token", firstSnapshot.AuthorizationBearerToken, "first bearer token");
            AssertEqual(string.Empty, secondSnapshot.CustomHeader, "second custom header");
            AssertEqual(string.Empty, secondSnapshot.Authorization, "second authorization");
            AssertEqual("second-value", thirdSnapshot.CustomHeader, "third custom header");
            AssertEqual("second-token", thirdSnapshot.AuthorizationBearerToken, "third bearer token");
        }

        private static async Task TestExternalTimeoutConflictAsync(CancellationToken cancellationToken)
        {
            await ExecuteExternalConflictAsync(request =>
            {
                request.TimeoutMilliseconds = 100;
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestExternalAutoRedirectConflictAsync(CancellationToken cancellationToken)
        {
            await ExecuteExternalConflictAsync(request =>
            {
                request.AllowAutoRedirect = false;
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestExternalIgnoreCertificateConflictAsync(CancellationToken cancellationToken)
        {
            await ExecuteExternalConflictAsync(request =>
            {
                request.IgnoreCertificateErrors = true;
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestExternalCertificateFilenameConflictAsync(CancellationToken cancellationToken)
        {
            await ExecuteExternalConflictAsync(request =>
            {
                request.CertificateFilename = "test-client.pfx";
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestExternalCertificatePasswordConflictAsync(CancellationToken cancellationToken)
        {
            await ExecuteExternalConflictAsync(request =>
            {
                request.CertificatePassword = "secret";
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task ExecuteExternalConflictAsync(Action<RestRequest> configureRequest, CancellationToken cancellationToken)
        {
            await using RestWrapperTestServer server = await RestWrapperTestServer.StartAsync(cancellationToken).ConfigureAwait(false);
            using HttpClient client = server.CreateHttpClient();
            using RestRequest request = new RestRequest(server.GetUrl("/text"), client);

            configureRequest(request);

            await AssertThrowsAsync<InvalidOperationException>(async () =>
            {
                using RestResponse response = await request.SendAsync(cancellationToken).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        private static async Task ExecuteWithRequestAsync(
            RequestTransportMode mode,
            string path,
            HttpMethod method,
            Func<RestWrapperTestServer, HttpClient?, RestRequest, CancellationToken, Task> executeAsync,
            CancellationToken cancellationToken)
        {
            await using RestWrapperTestServer server = await RestWrapperTestServer.StartAsync(cancellationToken).ConfigureAwait(false);
            HttpClient? client = null;

            try
            {
                if (mode == RequestTransportMode.External)
                    client = server.CreateHttpClient();

                using RestRequest request = CreateRequest(server, path, method, mode, client);
                await executeAsync(server, client, request, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                client?.Dispose();
            }
        }

        private static RestRequest CreateRequest(
            RestWrapperTestServer server,
            string path,
            HttpMethod method,
            RequestTransportMode mode,
            HttpClient? client)
        {
            string url = server.GetUrl(path);

            if (mode == RequestTransportMode.External)
                return new RestRequest(url, method, client ?? throw new ArgumentNullException(nameof(client)));

            return new RestRequest(url, method);
        }

        private static T DeserializeJson<T>(RestResponse response) where T : class, new()
        {
            T? result = response.DataFromJson<T>();
            if (result == null) throw new InvalidOperationException("Failed to deserialize JSON response.");
            return result;
        }

        private static string ModeId(RequestTransportMode mode)
        {
            return mode == RequestTransportMode.Internal ? "Internal" : "External";
        }

        private static string ModeLabel(RequestTransportMode mode)
        {
            return mode == RequestTransportMode.Internal ? "internal HttpClient" : "caller-supplied HttpClient";
        }

        private static void AssertStatus(int expected, int actual)
        {
            if (expected != actual)
                throw new InvalidOperationException("Expected status " + expected.ToString() + " but received " + actual.ToString() + ".");
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void AssertEqual(string expected, string actual, string label)
        {
            if (!String.Equals(expected, actual, StringComparison.Ordinal))
                throw new InvalidOperationException("Expected " + label + " to be '" + expected + "' but was '" + actual + "'.");
        }

        private static void AssertEqual(int expected, int actual, string label)
        {
            if (expected != actual)
                throw new InvalidOperationException("Expected " + label + " to be " + expected.ToString() + " but was " + actual.ToString() + ".");
        }

        private static void AssertThrows<TException>(Action action) where TException : Exception
        {
            try
            {
                action();
                throw new InvalidOperationException("Expected exception of type " + typeof(TException).Name + " was not thrown.");
            }
            catch (TException)
            {
            }
        }

        private static async Task AssertThrowsAsync<TException>(Func<Task> action) where TException : Exception
        {
            try
            {
                await action().ConfigureAwait(false);
                throw new InvalidOperationException("Expected exception of type " + typeof(TException).Name + " was not thrown.");
            }
            catch (TException)
            {
            }
        }
    }
}
