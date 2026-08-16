namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using RestWrapper;

    using Touchstone.Core;

    /// <summary>
    /// Additional suites that broaden coverage across helper utilities, synthetic responses, streaming edge cases, and transport-specific behaviors.
    /// </summary>
    public static partial class RestWrapperSuites
    {
        private static readonly object SerializerSettingsLock = new object();

        private static TestSuiteDescriptor HelperSuite()
        {
            string suiteId = "Helpers";

            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Constructors, helpers, and object model guards",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "AuthorizationNullResets", "Authorization property resets to a default instance when set to null", ct => TestAuthorizationNullResetsAsync()),
                    new TestCaseDescriptor(suiteId, "HeadersNullResets", "Headers property resets to an empty collection when set to null", ct => TestHeadersNullResetsAsync()),
                    new TestCaseDescriptor(suiteId, "ContentTypeConstructorAssigns", "Content-type constructor populates request state", ct => TestContentTypeConstructorAssignsAsync()),
                    new TestCaseDescriptor(suiteId, "HeadersContentTypeConstructorAssigns", "Headers/content-type constructor populates request state", ct => TestHeadersContentTypeConstructorAssignsAsync()),
                    new TestCaseDescriptor(suiteId, "RequestToStringIncludesFields", "RestRequest.ToString includes key configured fields", ct => TestRequestToStringIncludesFieldsAsync()),
                    new TestCaseDescriptor(suiteId, "ContentTypeParserMediaTypeCharset", "ContentTypeParser extracts media type and charset", ct => TestContentTypeParserMediaTypeCharsetAsync()),
                    new TestCaseDescriptor(suiteId, "ContentTypeParserBoundaryQuoted", "ContentTypeParser handles quoted parameters and boundaries", ct => TestContentTypeParserBoundaryQuotedAsync()),
                    new TestCaseDescriptor(suiteId, "ContentTypeParserMissingValues", "ContentTypeParser handles null and empty values cleanly", ct => TestContentTypeParserMissingValuesAsync()),
                    new TestCaseDescriptor(suiteId, "DefaultSerializerDeserialize", "Default serializer deserializes JSON", ct => TestDefaultSerializerDeserializeAsync()),
                    new TestCaseDescriptor(suiteId, "DefaultSerializerPrettyIgnoreNull", "Default serializer pretty-prints and omits null properties when configured", ct => TestDefaultSerializerPrettyIgnoreNullAsync()),
                    new TestCaseDescriptor(suiteId, "DefaultSerializerIncludesNull", "Default serializer can include null properties", ct => TestDefaultSerializerIncludesNullAsync()),
                    new TestCaseDescriptor(suiteId, "DefaultSerializerOptionsRejectNull", "Default serializer rejects null options", ct => TestDefaultSerializerOptionsRejectNullAsync())
                });
        }

        private static TestSuiteDescriptor TransportExpansionSuite(RequestTransportMode mode)
        {
            string suiteId = "Coverage." + ModeId(mode);
            string modeLabel = ModeLabel(mode);

            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Expanded request/response coverage (" + modeLabel + ")",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "PostNullStringBody", "POST handles null string bodies using " + modeLabel, ct => TestPostNullStringBodyAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "PostNullByteArrayBody", "POST handles null byte[] bodies using " + modeLabel, ct => TestPostNullByteArrayBodyAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "PostNullFormBody", "POST handles null form dictionaries using " + modeLabel, ct => TestPostNullFormBodyAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "GetIgnoresBody", "GET ignores payload bodies using " + modeLabel, ct => TestGetIgnoresBodyAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "SeekableStreamRewinds", "Seekable streams rewind before send using " + modeLabel, ct => TestSeekableStreamRewindsAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "PlaintextBasicAuthorization", "Plaintext basic authorization is supported using " + modeLabel, ct => TestPlaintextBasicAuthorizationAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "ContentHeadersPropagate", "Content headers propagate using " + modeLabel, ct => TestContentHeadersPropagateAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "DefaultRequestHeadersPresent", "Default Accept and Host headers are present using " + modeLabel, ct => TestDefaultRequestHeadersPresentAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "RedirectFollowedByDefault", "Redirects are followed by default using " + modeLabel, ct => TestRedirectFollowedByDefaultAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "ChunkedRequestBodyArrivesIntact", "Chunked request payloads serialize correctly using " + modeLabel, ct => TestChunkedRequestBodyArrivesIntactAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "ChunkedResponseReadAfterFinalReturnsNull", "Chunked responses report final chunks and then EOF using " + modeLabel, ct => TestChunkedResponseReadAfterFinalReturnsNullAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "ServerSentEventsComplex", "Complex SSE payloads parse correctly using " + modeLabel, ct => TestServerSentEventsComplexAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "HeadDataFromJsonThrows", "HEAD responses reject JSON deserialization using " + modeLabel, ct => TestHeadDataFromJsonThrowsAsync(mode, ct)),
                    new TestCaseDescriptor(suiteId, "ResponseMetadataAccessible", "Response metadata remains accessible using " + modeLabel, ct => TestResponseMetadataAccessibleAsync(mode, ct))
                });
        }

        private static TestSuiteDescriptor DirectResponseSuite()
        {
            string suiteId = "DirectResponse";

            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Synthetic RestResponse behavior",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "SyntheticChunkedResponseParsesChunks", "Synthetic chunked responses parse chunks, final flags, and EOF", ct => TestSyntheticChunkedResponseParsesChunksAsync()),
                    new TestCaseDescriptor(suiteId, "SyntheticChunkedResponseGuards", "Synthetic chunked responses guard regular accessors", ct => TestSyntheticChunkedResponseGuardsAsync()),
                    new TestCaseDescriptor(suiteId, "SyntheticSseResponseParsesEvents", "Synthetic SSE responses parse events and EOF", ct => TestSyntheticSseResponseParsesEventsAsync()),
                    new TestCaseDescriptor(suiteId, "SyntheticSseResponseGuards", "Synthetic SSE responses guard regular accessors", ct => TestSyntheticSseResponseGuardsAsync()),
                    new TestCaseDescriptor(suiteId, "SyntheticNormalResponseRejectsStreamingReads", "Normal responses reject streaming readers", ct => TestSyntheticNormalResponseRejectsStreamingReadsAsync()),
                    new TestCaseDescriptor(suiteId, "SyntheticSerializationHelperRejectsNull", "RestResponse rejects null serialization helpers", ct => TestSyntheticSerializationHelperRejectsNullAsync()),
                    new TestCaseDescriptor(suiteId, "SyntheticToStringIncludesMarkers", "RestResponse.ToString includes streaming markers", ct => TestSyntheticToStringIncludesMarkersAsync()),
                    new TestCaseDescriptor(suiteId, "SyntheticHeadLeavesDataNull", "Synthetic HEAD responses do not expose a body stream", ct => TestSyntheticHeadLeavesDataNullAsync()),
                    new TestCaseDescriptor(suiteId, "SyntheticCharacterSetAndHeaders", "Synthetic responses expose character set and headers", ct => TestSyntheticCharacterSetAndHeadersAsync()),
                    new TestCaseDescriptor(suiteId, "SyntheticSseCancellationHonored", "Synthetic SSE reads honor cancellation", ct => TestSyntheticSseCancellationHonoredAsync()),
                    new TestCaseDescriptor(suiteId, "SyntheticSuccessStatusFlags", "Synthetic responses expose success status flags", ct => TestSyntheticSuccessStatusFlagsAsync()),
                    new TestCaseDescriptor(suiteId, "SyntheticEmptyBodyDataFromJsonThrows", "DataFromJson throws when the response body is empty", ct => TestSyntheticEmptyBodyDataFromJsonThrowsAsync()),
                    new TestCaseDescriptor(suiteId, "SyntheticMalformedJsonThrows", "DataFromJson throws when the response body is not valid JSON", ct => TestSyntheticMalformedJsonThrowsAsync()),
                    new TestCaseDescriptor(suiteId, "SyntheticContentEncodingSurfaced", "Responses surface the Content-Encoding header", ct => TestSyntheticContentEncodingSurfacedAsync())
                });
        }

        private static TestSuiteDescriptor ServerSentEventReaderSuite()
        {
            string suiteId = "ServerSentEventReader";

            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Direct ServerSentEventReader coverage",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "NullStreamThrows", "ServerSentEventReader rejects null streams", ct => TestServerSentEventReaderNullStreamThrowsAsync()),
                    new TestCaseDescriptor(suiteId, "ParsesComplexPayload", "ServerSentEventReader parses comments, multiline data, and metadata", ct => TestServerSentEventReaderParsesComplexPayloadAsync()),
                    new TestCaseDescriptor(suiteId, "EofWithoutBlankLine", "ServerSentEventReader returns the final event at EOF without a trailing blank line", ct => TestServerSentEventReaderEofWithoutBlankLineAsync()),
                    new TestCaseDescriptor(suiteId, "SyncReadWorks", "ServerSentEventReader synchronous reads work", ct => TestServerSentEventReaderSyncReadWorksAsync()),
                    new TestCaseDescriptor(suiteId, "CancellationHonored", "ServerSentEventReader honors cancellation while waiting for data", ct => TestServerSentEventReaderCancellationHonoredAsync())
                });
        }

        private static TestSuiteDescriptor InternalTransportFeatureSuite()
        {
            string suiteId = "InternalTransportFeatures";

            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Internally-owned transport features",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "RedirectDisabledReturns302", "AllowAutoRedirect=false returns the redirect response for internal clients", ct => TestInternalRedirectDisabledAsync(ct))
                });
        }

        private static TestSuiteDescriptor ExternalTransportFeatureSuite()
        {
            string suiteId = "ExternalTransportFeatures";

            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Caller-configured external transport features",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "CallerConfiguredNoRedirectReturns302", "A caller-supplied HttpClient handler can disable redirects", ct => TestExternalConfiguredNoRedirectAsync(ct))
                });
        }

        private static Task TestAuthorizationNullResetsAsync()
        {
            using RestRequest request = new RestRequest("http://127.0.0.1");
            request.Authorization = null!;
            AssertNotNull(request.Authorization, "Authorization");
            AssertEqual(string.Empty, request.Authorization.User ?? string.Empty, "Authorization.User");
            return Task.CompletedTask;
        }

        private static Task TestHeadersNullResetsAsync()
        {
            using RestRequest request = new RestRequest("http://127.0.0.1");
            request.Headers = null!;
            AssertNotNull(request.Headers, "Headers");
            AssertEqual(0, request.Headers.Count, "Headers.Count");
            return Task.CompletedTask;
        }

        private static Task TestContentTypeConstructorAssignsAsync()
        {
            using RestRequest request = new RestRequest("http://127.0.0.1/test", HttpMethod.Post, "application/json");
            AssertEqual("http://127.0.0.1/test", request.Url, "Url");
            AssertEqual("POST", request.Method.Method, "Method");
            AssertEqual("application/json", request.ContentType ?? string.Empty, "ContentType");
            return Task.CompletedTask;
        }

        private static Task TestHeadersContentTypeConstructorAssignsAsync()
        {
            NameValueCollection headers = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase)
            {
                { "X-Test", "value" }
            };

            using HttpClient client = new HttpClient();
            using RestRequest request = new RestRequest("http://127.0.0.1/test", HttpMethod.Put, headers, "text/plain", client);

            AssertEqual("http://127.0.0.1/test", request.Url, "Url");
            AssertEqual("PUT", request.Method.Method, "Method");
            AssertEqual("text/plain", request.ContentType ?? string.Empty, "ContentType");
            AssertEqual("value", request.Headers["X-Test"] ?? string.Empty, "X-Test");
            return Task.CompletedTask;
        }

        private static Task TestRequestToStringIncludesFieldsAsync()
        {
            using RestRequest request = new RestRequest("http://127.0.0.1/test", HttpMethod.Post, "application/json");
            request.Headers.Add("X-Test", "value");
            request.Authorization.BearerToken = "token-123";
            request.ContentLength = 15;
            request.AllowAutoRedirect = false;

            string output = request.ToString();

            AssertContains("Method             : POST", output, "request string");
            AssertContains("URL                : http://127.0.0.1/test", output, "request string");
            AssertContains("Bearer Token     : token-123", output, "request string");
            AssertContains("X-Test: value", output, "request string");
            AssertContains("Auto Redirect      : False", output, "request string");
            return Task.CompletedTask;
        }

        private static Task TestContentTypeParserMediaTypeCharsetAsync()
        {
            string contentType = "application/json; charset=utf-8";
            AssertEqual("application/json", ContentTypeParser.ExtractMediaType(contentType) ?? string.Empty, "media type");
            AssertEqual("utf-8", ContentTypeParser.ExtractCharset(contentType) ?? string.Empty, "charset");
            AssertEqual("utf-8", ContentTypeParser.ExtractParameter(contentType, "charset") ?? string.Empty, "charset parameter");
            return Task.CompletedTask;
        }

        private static Task TestContentTypeParserBoundaryQuotedAsync()
        {
            string contentType = "multipart/form-data; boundary=\"----abc123\"; charset=utf-8";
            AssertEqual("----abc123", ContentTypeParser.ExtractBoundary(contentType) ?? string.Empty, "boundary");
            AssertEqual("utf-8", ContentTypeParser.ExtractParameter(contentType, "charset") ?? string.Empty, "charset");
            AssertEqual(2, ContentTypeParser.ExtractParameters(contentType).Count, "parameter count");
            return Task.CompletedTask;
        }

        private static Task TestContentTypeParserMissingValuesAsync()
        {
            AssertNull(ContentTypeParser.ExtractMediaType(null!), "null media type");
            AssertNull(ContentTypeParser.ExtractCharset(String.Empty), "empty charset");
            AssertNull(ContentTypeParser.ExtractBoundary("text/plain"), "boundary");
            AssertEqual(0, ContentTypeParser.ExtractParameters(null!).Count, "null parameter count");
            return Task.CompletedTask;
        }

        private static Task TestDefaultSerializerDeserializeAsync()
        {
            DefaultSerializationHelper serializer = new DefaultSerializationHelper();
            SamplePayload payload = serializer.DeserializeJson<SamplePayload>("{\"Name\":\"alpha\",\"Value\":5}");
            AssertEqual("alpha", payload.Name, "Name");
            AssertEqual(5, payload.Value, "Value");
            return Task.CompletedTask;
        }

        private static Task TestDefaultSerializerPrettyIgnoreNullAsync()
        {
            lock (SerializerSettingsLock)
            {
                bool originalPretty = DefaultSerializationHelper.Pretty;
                bool originalIgnoreNull = DefaultSerializationHelper.IgnoreNull;
                JsonSerializerOptions originalOptions = DefaultSerializationHelper.Options;

                try
                {
                    DefaultSerializationHelper.Pretty = true;
                    DefaultSerializationHelper.IgnoreNull = true;
                    DefaultSerializationHelper.Options = new JsonSerializerOptions();

                    DefaultSerializationHelper serializer = new DefaultSerializationHelper();
                    string json = serializer.SerializeJson(new SerializationProbe
                    {
                        Name = "probe",
                        Optional = null
                    });

                    AssertContains("\n", json, "pretty JSON");
                    AssertTrue(!json.Contains("Optional", StringComparison.Ordinal), "Null property should be omitted");
                }
                finally
                {
                    DefaultSerializationHelper.Pretty = originalPretty;
                    DefaultSerializationHelper.IgnoreNull = originalIgnoreNull;
                    DefaultSerializationHelper.Options = originalOptions;
                }
            }

            return Task.CompletedTask;
        }

        private static Task TestDefaultSerializerIncludesNullAsync()
        {
            lock (SerializerSettingsLock)
            {
                bool originalPretty = DefaultSerializationHelper.Pretty;
                bool originalIgnoreNull = DefaultSerializationHelper.IgnoreNull;
                JsonSerializerOptions originalOptions = DefaultSerializationHelper.Options;

                try
                {
                    DefaultSerializationHelper.Pretty = false;
                    DefaultSerializationHelper.IgnoreNull = false;
                    DefaultSerializationHelper.Options = new JsonSerializerOptions();

                    DefaultSerializationHelper serializer = new DefaultSerializationHelper();
                    string json = serializer.SerializeJson(new SerializationProbe
                    {
                        Name = "probe",
                        Optional = null
                    }, pretty: false);

                    AssertContains("\"Optional\":null", json, "JSON output");
                }
                finally
                {
                    DefaultSerializationHelper.Pretty = originalPretty;
                    DefaultSerializationHelper.IgnoreNull = originalIgnoreNull;
                    DefaultSerializationHelper.Options = originalOptions;
                }
            }

            return Task.CompletedTask;
        }

        private static Task TestDefaultSerializerOptionsRejectNullAsync()
        {
            lock (SerializerSettingsLock)
            {
                JsonSerializerOptions originalOptions = DefaultSerializationHelper.Options;

                try
                {
                    AssertThrows<ArgumentNullException>(() =>
                    {
                        DefaultSerializationHelper.Options = null!;
                    });
                }
                finally
                {
                    DefaultSerializationHelper.Options = originalOptions;
                }
            }

            return Task.CompletedTask;
        }

        private static async Task TestPostNullStringBodyAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/echo", HttpMethod.Post, async (server, client, request, ct) =>
            {
                request.ContentType = "text/plain";
                using RestResponse response = await request.SendAsync((string)null!, ct).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);
                AssertEqual(string.Empty, response.DataAsString ?? string.Empty, "echo payload");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestPostNullByteArrayBodyAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/echo", HttpMethod.Post, async (server, client, request, ct) =>
            {
                request.ContentType = "application/octet-stream";
                using RestResponse response = await request.SendAsync((byte[])null!, ct).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);
                AssertEqual(string.Empty, response.DataAsString ?? string.Empty, "echo payload");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestPostNullFormBodyAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/echo", HttpMethod.Post, async (server, client, request, ct) =>
            {
                using RestResponse response = await request.SendAsync((Dictionary<string, string>)null!, ct).ConfigureAwait(false);
                AssertStatus(200, response.StatusCode);
                AssertEqual(string.Empty, response.DataAsString ?? string.Empty, "echo payload");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestGetIgnoresBodyAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/methods", HttpMethod.Get, async (server, client, request, ct) =>
            {
                using RestResponse response = await request.SendAsync("ignored body", ct).ConfigureAwait(false);
                RequestSnapshot snapshot = DeserializeJson<RequestSnapshot>(response);

                AssertEqual("GET", snapshot.Method, "method");
                AssertEqual(string.Empty, snapshot.Body, "body");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestSeekableStreamRewindsAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/methods", HttpMethod.Put, async (server, client, request, ct) =>
            {
                request.ContentType = "text/plain";
                byte[] payload = Encoding.UTF8.GetBytes("rewound payload");

                using MemoryStream stream = new MemoryStream(payload);
                stream.Position = 7;

                using RestResponse response = await request.SendAsync(payload.Length, stream, ct).ConfigureAwait(false);
                RequestSnapshot snapshot = DeserializeJson<RequestSnapshot>(response);

                AssertEqual("rewound payload", snapshot.Body, "body");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestPlaintextBasicAuthorizationAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/inspect", HttpMethod.Get, async (server, client, request, ct) =>
            {
                request.Authorization.User = "plain-user";
                request.Authorization.Password = "plain-pass";
                request.Authorization.EncodeCredentials = false;

                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                RequestSnapshot snapshot = DeserializeJson<RequestSnapshot>(response);

                AssertEqual("Basic plain-user:plain-pass", snapshot.Authorization, "authorization");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestContentHeadersPropagateAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/inspect", HttpMethod.Post, async (server, client, request, ct) =>
            {
                request.ContentType = "text/plain";
                request.Headers.Add("Content-Type", "application/json; charset=utf-8");
                request.Headers.Add("Content-Language", "en-US");

                using RestResponse response = await request.SendAsync("{\"value\":1}", ct).ConfigureAwait(false);
                RequestSnapshot snapshot = DeserializeJson<RequestSnapshot>(response);

                AssertContains("application/json", snapshot.ContentType, "content type");
                AssertContains("utf-8", snapshot.ContentType, "content type");
                AssertEqual("en-US", snapshot.ContentLanguage, "content language");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestDefaultRequestHeadersPresentAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/inspect", HttpMethod.Get, async (server, client, request, ct) =>
            {
                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                RequestSnapshot snapshot = DeserializeJson<RequestSnapshot>(response);

                AssertContains("*/*", snapshot.Accept, "Accept");
                AssertTrue(!String.IsNullOrEmpty(snapshot.Host), "Host header should be present");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestRedirectFollowedByDefaultAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/redirect-text", HttpMethod.Get, async (server, client, request, ct) =>
            {
                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);

                AssertStatus(200, response.StatusCode);
                AssertEqual("Sample text response for testing", response.DataAsString ?? string.Empty, "redirect body");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestChunkedRequestBodyArrivesIntactAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            using InspectableChunkedContent content = new InspectableChunkedContent();

            await content.SendChunk("alpha ", cancellationToken).ConfigureAwait(false);
            await content.SendChunk("beta ", cancellationToken).ConfigureAwait(false);
            await content.SendChunk("gamma", cancellationToken).ConfigureAwait(false);
            await content.SendEmptyChunk(cancellationToken).ConfigureAwait(false);

            string serialized = await content.SerializeToStringAsync().ConfigureAwait(false);
            AssertEqual("6\r\nalpha \r\n5\r\nbeta \r\n5\r\ngamma\r\n0\r\n\r\n", serialized, "serialized chunked content");
        }

        private static async Task TestChunkedResponseReadAfterFinalReturnsNullAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/chunked", HttpMethod.Post, async (server, client, request, ct) =>
            {
                request.ChunkedTransfer = true;

                using RestResponse response = await request.SendChunkAsync(Array.Empty<byte>(), true, ct).ConfigureAwait(false);

                ChunkData? first = await response.ReadChunkAsync(ct).ConfigureAwait(false);
                ChunkData? second = await response.ReadChunkAsync(ct).ConfigureAwait(false);
                ChunkData? third = await response.ReadChunkAsync(ct).ConfigureAwait(false);
                ChunkData? fourth = await response.ReadChunkAsync(ct).ConfigureAwait(false);
                ChunkData? fifth = await response.ReadChunkAsync(ct).ConfigureAwait(false);
                ChunkData? afterFinal = await response.ReadChunkAsync(ct).ConfigureAwait(false);

                AssertNotNull(first, "first chunk");
                AssertNotNull(second, "second chunk");
                AssertNotNull(third, "third chunk");
                AssertNotNull(fourth, "fourth chunk");
                AssertNotNull(fifth, "fifth chunk");
                AssertEqual(true, fifth!.IsFinal, "fifth.IsFinal");
                AssertNull(afterFinal, "after final chunk");
                AssertEqual("Server chunk 4", Encoding.UTF8.GetString(fifth.Data ?? Array.Empty<byte>()), "fifth data");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestServerSentEventsComplexAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/sse-complex", HttpMethod.Get, async (server, client, request, ct) =>
            {
                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);

                AssertEqual(true, response.ServerSentEvents, "ServerSentEvents");

                ServerSentEvent? first = await response.ReadEventAsync(ct).ConfigureAwait(false);
                ServerSentEvent? second = await response.ReadEventAsync(ct).ConfigureAwait(false);
                ServerSentEvent? third = await response.ReadEventAsync(ct).ConfigureAwait(false);
                ServerSentEvent? afterFinal = await response.ReadEventAsync(ct).ConfigureAwait(false);

                AssertNotNull(first, "first event");
                AssertNotNull(second, "second event");
                AssertNotNull(third, "third event");
                AssertNull(afterFinal, "after final event");

                AssertEqual("42", first!.Id ?? string.Empty, "first.Id");
                AssertEqual("update", first.Event ?? string.Empty, "first.Event");
                AssertEqual(1500, first.Retry ?? 0, "first.Retry");
                AssertEqual("line 1\nline 2", first.Data ?? string.Empty, "first.Data");

                AssertEqual("heartbeat", second!.Event ?? string.Empty, "second.Event");
                AssertEqual(string.Empty, second.Data ?? string.Empty, "second.Data");

                AssertEqual("99", third!.Id ?? string.Empty, "third.Id");
                AssertEqual("final payload", third.Data ?? string.Empty, "third.Data");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestHeadDataFromJsonThrowsAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/methods", HttpMethod.Head, async (server, client, request, ct) =>
            {
                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);
                await AssertThrowsAsync<InvalidOperationException>(async () =>
                {
                    SamplePayload payload = response.DataFromJson<SamplePayload>();
                    await Task.FromResult(payload).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestResponseMetadataAccessibleAsync(RequestTransportMode mode, CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(mode, "/text", HttpMethod.Get, async (server, client, request, ct) =>
            {
                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);

                AssertStatus(200, response.StatusCode);
                AssertEqual(true, response.IsSuccessStatusCode, "IsSuccessStatusCode");
                AssertEqual("text/plain", response.ContentType ?? string.Empty, "ContentType");
                AssertEqual("utf-8", response.CharacterSet ?? string.Empty, "CharacterSet");
                AssertContains("HTTP/", response.ProtocolVersion ?? string.Empty, "ProtocolVersion");
                AssertEqual("RestWrapperTestServer", response.Headers["X-Test-Server"] ?? string.Empty, "X-Test-Server");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static Task TestSyntheticChunkedResponseParsesChunksAsync()
        {
            using RestResponse response = CreateSyntheticResponse(
                method: HttpMethod.Get,
                statusCode: HttpStatusCode.OK,
                contentType: "text/plain",
                body: "alpha\nbeta\ncharlie",
                chunked: true);

            return ExecuteSyntheticChunkAssertionsAsync(response);
        }

        private static async Task ExecuteSyntheticChunkAssertionsAsync(RestResponse response)
        {
            ChunkData? first = await response.ReadChunkAsync().ConfigureAwait(false);
            ChunkData? second = await response.ReadChunkAsync().ConfigureAwait(false);
            ChunkData? third = await response.ReadChunkAsync().ConfigureAwait(false);
            ChunkData? afterFinal = await response.ReadChunkAsync().ConfigureAwait(false);

            AssertNotNull(first, "first chunk");
            AssertNotNull(second, "second chunk");
            AssertNotNull(third, "third chunk");
            AssertNull(afterFinal, "after final chunk");

            AssertEqual("alpha", Encoding.UTF8.GetString(first!.Data ?? Array.Empty<byte>()), "first data");
            AssertEqual(false, first.IsFinal, "first.IsFinal");
            AssertEqual("beta", Encoding.UTF8.GetString(second!.Data ?? Array.Empty<byte>()), "second data");
            AssertEqual(false, second.IsFinal, "second.IsFinal");
            AssertEqual("charlie", Encoding.UTF8.GetString(third!.Data ?? Array.Empty<byte>()), "third data");
            AssertEqual(true, third.IsFinal, "third.IsFinal");
        }

        private static Task TestSyntheticChunkedResponseGuardsAsync()
        {
            using RestResponse response = CreateSyntheticResponse(
                method: HttpMethod.Get,
                statusCode: HttpStatusCode.OK,
                contentType: "text/plain",
                body: "alpha\nbeta",
                chunked: true);

            AssertThrows<InvalidOperationException>(() =>
            {
                Stream? data = response.Data;
            });

            AssertThrows<InvalidOperationException>(() =>
            {
                string? data = response.DataAsString;
            });

            AssertThrows<InvalidOperationException>(() =>
            {
                byte[]? data = response.DataAsBytes;
            });

            AssertThrows<InvalidOperationException>(() =>
            {
                SamplePayload payload = response.DataFromJson<SamplePayload>();
            });

            return Task.CompletedTask;
        }

        private static async Task TestSyntheticSseResponseParsesEventsAsync()
        {
            using RestResponse response = CreateSyntheticResponse(
                method: HttpMethod.Get,
                statusCode: HttpStatusCode.OK,
                contentType: "text/event-stream",
                body: "id: 7\nevent: update\ndata: line 1\ndata: line 2\n\nid: 8\ndata: tail");

            ServerSentEvent? first = await response.ReadEventAsync().ConfigureAwait(false);
            ServerSentEvent? second = await response.ReadEventAsync().ConfigureAwait(false);
            ServerSentEvent? afterFinal = await response.ReadEventAsync().ConfigureAwait(false);

            AssertNotNull(first, "first event");
            AssertNotNull(second, "second event");
            AssertNull(afterFinal, "after final event");

            AssertEqual("7", first!.Id ?? string.Empty, "first.Id");
            AssertEqual("update", first.Event ?? string.Empty, "first.Event");
            AssertEqual("line 1\nline 2", first.Data ?? string.Empty, "first.Data");

            AssertEqual("8", second!.Id ?? string.Empty, "second.Id");
            AssertEqual("tail", second.Data ?? string.Empty, "second.Data");
        }

        private static Task TestSyntheticSseResponseGuardsAsync()
        {
            using RestResponse response = CreateSyntheticResponse(
                method: HttpMethod.Get,
                statusCode: HttpStatusCode.OK,
                contentType: "text/event-stream",
                body: "data: hello\n\n");

            AssertThrows<InvalidOperationException>(() =>
            {
                Stream? data = response.Data;
            });

            AssertThrows<InvalidOperationException>(() =>
            {
                string? data = response.DataAsString;
            });

            AssertThrows<InvalidOperationException>(() =>
            {
                byte[]? data = response.DataAsBytes;
            });

            AssertThrows<InvalidOperationException>(() =>
            {
                SamplePayload payload = response.DataFromJson<SamplePayload>();
            });

            return Task.CompletedTask;
        }

        private static async Task TestSyntheticNormalResponseRejectsStreamingReadsAsync()
        {
            using RestResponse response = CreateSyntheticResponse(
                method: HttpMethod.Get,
                statusCode: HttpStatusCode.OK,
                contentType: "text/plain",
                body: "hello world");

            await AssertThrowsAsync<InvalidOperationException>(async () =>
            {
                ServerSentEvent? evt = await response.ReadEventAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);

            await AssertThrowsAsync<InvalidOperationException>(async () =>
            {
                ChunkData? chunk = await response.ReadChunkAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        private static Task TestSyntheticSerializationHelperRejectsNullAsync()
        {
            using RestResponse response = CreateSyntheticResponse(
                method: HttpMethod.Get,
                statusCode: HttpStatusCode.OK,
                contentType: "application/json",
                body: "{\"Name\":\"alpha\",\"Value\":5}");

            AssertThrows<ArgumentNullException>(() =>
            {
                response.SerializationHelper = null!;
            });

            return Task.CompletedTask;
        }

        private static Task TestSyntheticToStringIncludesMarkersAsync()
        {
            using RestResponse chunked = CreateSyntheticResponse(
                method: HttpMethod.Get,
                statusCode: HttpStatusCode.OK,
                contentType: "text/plain",
                body: "alpha\nbeta",
                chunked: true);

            using RestResponse sse = CreateSyntheticResponse(
                method: HttpMethod.Get,
                statusCode: HttpStatusCode.OK,
                contentType: "text/event-stream",
                body: "data: hello\n\n");

            AssertContains("[chunked]", chunked.ToString(), "chunked ToString");
            AssertContains("[server-sent events]", sse.ToString(), "sse ToString");
            return Task.CompletedTask;
        }

        private static Task TestSyntheticHeadLeavesDataNullAsync()
        {
            using RestResponse response = CreateSyntheticResponse(
                method: HttpMethod.Head,
                statusCode: HttpStatusCode.OK,
                contentType: "application/json",
                body: "{\"ignored\":true}");

            AssertNull(response.Data, "Data");
            AssertNull(response.DataAsString, "DataAsString");
            return Task.CompletedTask;
        }

        private static Task TestSyntheticCharacterSetAndHeadersAsync()
        {
            using RestResponse response = CreateSyntheticResponse(
                method: HttpMethod.Get,
                statusCode: HttpStatusCode.OK,
                contentType: "application/json; charset=utf-8",
                body: "{\"Name\":\"alpha\",\"Value\":1}",
                headers: new Dictionary<string, string>
                {
                    { "X-Synthetic-Test", "yes" }
                });

            AssertEqual("application/json", response.ContentType ?? string.Empty, "ContentType");
            AssertEqual("utf-8", response.CharacterSet ?? string.Empty, "CharacterSet");
            AssertEqual("yes", response.Headers["X-Synthetic-Test"] ?? string.Empty, "X-Synthetic-Test");
            return Task.CompletedTask;
        }

        private static async Task TestSyntheticSseCancellationHonoredAsync()
        {
            using RestResponse response = CreateSyntheticResponse(
                method: HttpMethod.Get,
                statusCode: HttpStatusCode.OK,
                contentType: "text/event-stream",
                body: null,
                stream: new CancellationBlockingStream());

            using CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.CancelAfter(50);

            await AssertThrowsAsync<OperationCanceledException>(async () =>
            {
                ServerSentEvent? evt = await response.ReadEventAsync(cancellation.Token).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        private static Task TestSyntheticSuccessStatusFlagsAsync()
        {
            using RestResponse success = CreateSyntheticResponse(
                method: HttpMethod.Get,
                statusCode: HttpStatusCode.OK,
                contentType: "text/plain",
                body: "ok");

            using RestResponse notFound = CreateSyntheticResponse(
                method: HttpMethod.Get,
                statusCode: HttpStatusCode.NotFound,
                contentType: "text/plain",
                body: "missing");

            AssertEqual(true, success.IsSuccessStatusCode, "success.IsSuccessStatusCode");
            AssertEqual(false, notFound.IsSuccessStatusCode, "notFound.IsSuccessStatusCode");
            return Task.CompletedTask;
        }

        private static Task TestSyntheticEmptyBodyDataFromJsonThrowsAsync()
        {
            using RestResponse response = CreateSyntheticResponse(
                method: HttpMethod.Get,
                statusCode: HttpStatusCode.OK,
                contentType: "application/json",
                body: string.Empty);

            AssertThrows<InvalidOperationException>(() =>
            {
                SamplePayload payload = response.DataFromJson<SamplePayload>();
            });

            return Task.CompletedTask;
        }

        private static Task TestSyntheticMalformedJsonThrowsAsync()
        {
            using RestResponse response = CreateSyntheticResponse(
                method: HttpMethod.Get,
                statusCode: HttpStatusCode.OK,
                contentType: "application/json",
                body: "{ this is not valid json ");

            AssertThrows<JsonException>(() =>
            {
                SamplePayload payload = response.DataFromJson<SamplePayload>();
            });

            return Task.CompletedTask;
        }

        private static Task TestSyntheticContentEncodingSurfacedAsync()
        {
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Version = HttpVersion.Version11,
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://127.0.0.1/test")
            };

            message.Content = new ByteArrayContent(Encoding.UTF8.GetBytes("encoded-body"));
            message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
            message.Content.Headers.ContentEncoding.Add("gzip");

            using RestResponse response = new RestResponse(message);
            AssertEqual("gzip", response.ContentEncoding ?? string.Empty, "ContentEncoding");

            return Task.CompletedTask;
        }

        private static Task TestServerSentEventReaderNullStreamThrowsAsync()
        {
            AssertThrows<ArgumentNullException>(() =>
            {
                using ServerSentEventReader reader = new ServerSentEventReader(null!);
            });

            return Task.CompletedTask;
        }

        private static async Task TestServerSentEventReaderParsesComplexPayloadAsync()
        {
            using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(
                ": comment\nid: 42\nevent: update\nretry: 1500\ndata: line 1\ndata: line 2\n\n:event comment\n\nevent: heartbeat\n\nid: 99\ndata: final payload"));
            using ServerSentEventReader reader = new ServerSentEventReader(stream);

            ServerSentEvent? first = await reader.ReadNextEventAsync().ConfigureAwait(false);
            ServerSentEvent? second = await reader.ReadNextEventAsync().ConfigureAwait(false);
            ServerSentEvent? third = await reader.ReadNextEventAsync().ConfigureAwait(false);
            ServerSentEvent? afterFinal = await reader.ReadNextEventAsync().ConfigureAwait(false);

            AssertNotNull(first, "first event");
            AssertNotNull(second, "second event");
            AssertNotNull(third, "third event");
            AssertNull(afterFinal, "after final event");

            AssertEqual("42", first!.Id ?? string.Empty, "first.Id");
            AssertEqual("update", first.Event ?? string.Empty, "first.Event");
            AssertEqual(1500, first.Retry ?? 0, "first.Retry");
            AssertEqual("line 1\nline 2", first.Data ?? string.Empty, "first.Data");

            AssertEqual("heartbeat", second!.Event ?? string.Empty, "second.Event");
            AssertEqual(string.Empty, second.Data ?? string.Empty, "second.Data");

            AssertEqual("99", third!.Id ?? string.Empty, "third.Id");
            AssertEqual("final payload", third.Data ?? string.Empty, "third.Data");
        }

        private static async Task TestServerSentEventReaderEofWithoutBlankLineAsync()
        {
            using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("data: final payload"));
            using ServerSentEventReader reader = new ServerSentEventReader(stream);

            ServerSentEvent? evt = await reader.ReadNextEventAsync().ConfigureAwait(false);
            ServerSentEvent? afterFinal = await reader.ReadNextEventAsync().ConfigureAwait(false);

            AssertNotNull(evt, "event");
            AssertEqual("final payload", evt!.Data ?? string.Empty, "event.Data");
            AssertNull(afterFinal, "after final event");
        }

        private static Task TestServerSentEventReaderSyncReadWorksAsync()
        {
            using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("data: sync payload\n\n"));
            using ServerSentEventReader reader = new ServerSentEventReader(stream);

            ServerSentEvent evt = reader.ReadNextEvent();
            AssertNotNull(evt, "event");
            AssertEqual("sync payload", evt.Data ?? string.Empty, "event.Data");
            return Task.CompletedTask;
        }

        private static async Task TestServerSentEventReaderCancellationHonoredAsync()
        {
            using CancellationBlockingStream stream = new CancellationBlockingStream();
            using ServerSentEventReader reader = new ServerSentEventReader(stream);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.CancelAfter(50);

            await AssertThrowsAsync<OperationCanceledException>(async () =>
            {
                ServerSentEvent? evt = await reader.ReadNextEventAsync(cancellation.Token).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        private static async Task TestInternalRedirectDisabledAsync(CancellationToken cancellationToken)
        {
            await ExecuteWithRequestAsync(RequestTransportMode.Internal, "/redirect-text", HttpMethod.Get, async (server, client, request, ct) =>
            {
                request.AllowAutoRedirect = false;
                using RestResponse response = await request.SendAsync(ct).ConfigureAwait(false);

                AssertStatus(302, response.StatusCode);
                AssertEqual(false, response.IsSuccessStatusCode, "IsSuccessStatusCode");
                AssertEqual("/text", response.Headers["Location"] ?? string.Empty, "Location");
            }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TestExternalConfiguredNoRedirectAsync(CancellationToken cancellationToken)
        {
            await using RestWrapperTestServer server = await RestWrapperTestServer.StartAsync(cancellationToken).ConfigureAwait(false);
            using HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
            using HttpClient client = new HttpClient(handler);
            using RestRequest request = new RestRequest(server.GetUrl("/redirect-text"), HttpMethod.Get, client);
            using RestResponse response = await request.SendAsync(cancellationToken).ConfigureAwait(false);

            AssertStatus(302, response.StatusCode);
            AssertEqual(false, response.IsSuccessStatusCode, "IsSuccessStatusCode");
            AssertEqual("/text", response.Headers["Location"] ?? string.Empty, "Location");
        }

        private static RestResponse CreateSyntheticResponse(
            HttpMethod method,
            HttpStatusCode statusCode,
            string contentType,
            string? body,
            bool chunked = false,
            Dictionary<string, string>? headers = null,
            Stream? stream = null)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(statusCode)
            {
                Version = HttpVersion.Version11,
                RequestMessage = new HttpRequestMessage(method, "http://127.0.0.1/test")
            };

            Stream contentStream = stream ?? new MemoryStream(Encoding.UTF8.GetBytes(body ?? string.Empty));
            responseMessage.Content = new StreamContent(contentStream);
            responseMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

            if (chunked)
            {
                responseMessage.Headers.TransferEncodingChunked = true;
            }

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return new RestResponse(responseMessage);
        }

        private static void AssertContains(string expectedSubstring, string actual, string label)
        {
            if (actual == null || !actual.Contains(expectedSubstring, StringComparison.Ordinal))
                throw new InvalidOperationException("Expected " + label + " to contain '" + expectedSubstring + "' but was '" + actual + "'.");
        }

        private static void AssertEqual(bool expected, bool actual, string label)
        {
            if (expected != actual)
                throw new InvalidOperationException("Expected " + label + " to be " + expected.ToString() + " but was " + actual.ToString() + ".");
        }

        private static void AssertNull(object? value, string label)
        {
            if (value != null)
                throw new InvalidOperationException("Expected " + label + " to be null.");
        }

        private static void AssertNotNull(object? value, string label)
        {
            if (value == null)
                throw new InvalidOperationException("Expected " + label + " to be non-null.");
        }

        private sealed class SerializationProbe
        {
            public string Name { get; set; } = string.Empty;

            public string? Optional { get; set; }
        }

        private sealed class CancellationBlockingStream : Stream
        {
            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
                return 0;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
        }

        private sealed class InspectableChunkedContent : ChunkedContent
        {
            public async Task<string> SerializeToStringAsync()
            {
                using MemoryStream stream = new MemoryStream();
                await SerializeToStreamAsync(stream, null).ConfigureAwait(false);
                return Encoding.ASCII.GetString(stream.ToArray());
            }
        }
    }
}
