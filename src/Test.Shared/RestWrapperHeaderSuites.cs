namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using RestWrapper;

    using Touchstone.Core;

    /// <summary>
    /// Suites covering how response headers, including headers received more than once, are exposed on RestResponse.
    /// </summary>
    public static partial class RestWrapperSuites
    {
        private const string CookieOne = "session=abc123; Expires=Wed, 21 Oct 2026 07:28:00 GMT; Path=/";
        private const string CookieTwo = "theme=dark; Expires=Thu, 22 Oct 2026 07:28:00 GMT; Path=/; HttpOnly";

        private static TestSuiteDescriptor ResponseHeaderSuite()
        {
            string suiteId = "ResponseHeaders";

            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Response header values, including repeated headers",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "RepeatedHeaderGetValuesSeparate", "GetValues returns one entry per Set-Cookie even when values contain commas", ct => TestRepeatedHeaderGetValuesSeparateAsync()),
                    new TestCaseDescriptor(suiteId, "RepeatedHeaderIndexerJoined", "Headers[name] still returns repeated values comma-joined", ct => TestRepeatedHeaderIndexerJoinedAsync()),
                    new TestCaseDescriptor(suiteId, "RepeatedHeaderCaseInsensitive", "GetValues finds repeated headers regardless of name casing", ct => TestRepeatedHeaderCaseInsensitiveAsync()),
                    new TestCaseDescriptor(suiteId, "SingleHeaderWithCommaNotSplit", "A single header value containing commas is not split", ct => TestSingleHeaderWithCommaNotSplitAsync()),
                    new TestCaseDescriptor(suiteId, "MissingHeaderGetValuesNull", "GetValues returns null for a header that was not received", ct => TestMissingHeaderGetValuesNullAsync()),
                    new TestCaseDescriptor(suiteId, "SingleHeaderGetValuesOne", "GetValues returns exactly one entry for a header received once", ct => TestSingleHeaderGetValuesOneAsync()),
                    new TestCaseDescriptor(suiteId, "RepeatedHeaderThroughSendAsync", "Repeated Set-Cookie headers stay separate through RestRequest.SendAsync", ct => TestRepeatedHeaderThroughSendAsync(ct))
                });
        }

        private static Task TestRepeatedHeaderGetValuesSeparateAsync()
        {
            using RestResponse response = new RestResponse(CreateCookieResponse());

            string[]? values = response.Headers.GetValues("Set-Cookie");
            AssertNotNull(values, "Set-Cookie values");
            AssertEqual(2, values!.Length, "Set-Cookie value count");
            AssertEqual(CookieOne, values[0], "first Set-Cookie");
            AssertEqual(CookieTwo, values[1], "second Set-Cookie");
            return Task.CompletedTask;
        }

        private static Task TestRepeatedHeaderIndexerJoinedAsync()
        {
            using RestResponse response = new RestResponse(CreateCookieResponse());

            AssertEqual(CookieOne + "," + CookieTwo, response.Headers["Set-Cookie"] ?? string.Empty, "Set-Cookie indexer");
            return Task.CompletedTask;
        }

        private static Task TestRepeatedHeaderCaseInsensitiveAsync()
        {
            using RestResponse response = new RestResponse(CreateCookieResponse());

            string[]? values = response.Headers.GetValues("set-cookie");
            AssertNotNull(values, "set-cookie values");
            AssertEqual(2, values!.Length, "set-cookie value count");
            return Task.CompletedTask;
        }

        private static Task TestSingleHeaderWithCommaNotSplitAsync()
        {
            HttpResponseMessage message = CreateBareResponse();
            message.Headers.TryAddWithoutValidation("X-List", "alpha, beta, gamma");

            using RestResponse response = new RestResponse(message);

            string[]? values = response.Headers.GetValues("X-List");
            AssertNotNull(values, "X-List values");
            AssertEqual(1, values!.Length, "X-List value count");
            AssertEqual("alpha, beta, gamma", values[0], "X-List value");
            return Task.CompletedTask;
        }

        private static Task TestMissingHeaderGetValuesNullAsync()
        {
            using RestResponse response = new RestResponse(CreateCookieResponse());

            AssertNull(response.Headers.GetValues("X-Not-Present"), "X-Not-Present values");
            AssertNull(response.Headers["X-Not-Present"], "X-Not-Present indexer");
            return Task.CompletedTask;
        }

        private static Task TestSingleHeaderGetValuesOneAsync()
        {
            HttpResponseMessage message = CreateBareResponse();
            message.Headers.TryAddWithoutValidation("X-Single", "only");

            using RestResponse response = new RestResponse(message);

            string[]? values = response.Headers.GetValues("X-Single");
            AssertNotNull(values, "X-Single values");
            AssertEqual(1, values!.Length, "X-Single value count");
            AssertEqual("only", values[0], "X-Single value");
            return Task.CompletedTask;
        }

        private static async Task TestRepeatedHeaderThroughSendAsync(CancellationToken cancellationToken)
        {
            using CannedResponseHandler handler = new CannedResponseHandler(CreateCookieResponse);
            using HttpClient client = new HttpClient(handler);
            using RestRequest request = new RestRequest("http://127.0.0.1:1/cookies", HttpMethod.Get, client);
            using RestResponse response = await request.SendAsync(cancellationToken).ConfigureAwait(false);

            AssertStatus(200, response.StatusCode);
            string[]? values = response.Headers.GetValues("Set-Cookie");
            AssertNotNull(values, "Set-Cookie values");
            AssertEqual(2, values!.Length, "Set-Cookie value count");
            AssertEqual(CookieOne, values[0], "first Set-Cookie");
            AssertEqual(CookieTwo, values[1], "second Set-Cookie");
        }

        private static HttpResponseMessage CreateCookieResponse()
        {
            HttpResponseMessage message = CreateBareResponse();
            message.Headers.TryAddWithoutValidation("Set-Cookie", CookieOne);
            message.Headers.TryAddWithoutValidation("Set-Cookie", CookieTwo);
            return message;
        }

        private static HttpResponseMessage CreateBareResponse()
        {
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Version = HttpVersion.Version11,
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://127.0.0.1/test")
            };

            message.Content = new ByteArrayContent(Encoding.UTF8.GetBytes("ok"));
            message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
            return message;
        }

        private sealed class CannedResponseHandler : HttpMessageHandler
        {
            private readonly Func<HttpResponseMessage> _Factory;

            public CannedResponseHandler(Func<HttpResponseMessage> factory)
            {
                _Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                HttpResponseMessage response = _Factory();
                response.RequestMessage = request;
                return Task.FromResult(response);
            }
        }
    }
}
