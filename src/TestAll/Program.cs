namespace TestAll
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using RestWrapper;
    using WatsonWebserver;
    using WatsonWebserver.Core;
    using RestWrapperSerializationHelper = RestWrapper.ISerializationHelper;
    using HttpMethod = System.Net.Http.HttpMethod;

    public static class Program
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        private static List<TestResult> _TestResults = new List<TestResult>();
        private static Webserver _Webserver = null;
        private static string _WebserverUrl = "http://localhost:9000";

        public static async Task Main(string[] args)
        {
            Console.WriteLine("RestWrapper Comprehensive Test Suite");
            Console.WriteLine("=====================================");
            Console.WriteLine();

            var overallStopwatch = Stopwatch.StartNew();

            try
            {
                // Start test webserver
                await StartTestWebserver();

                // Run all test categories
                await RunStandardHttpTests();
                await RunStandardHttpsTests();
                await RunLocalHttpTests();
                await RunChunkedTransferTests();
                await RunServerSentEventTests();
                await RunSerializationTests();
                await RunDataPropertyTests();
                await RunCancellationTests();
                await RunNegativeTests();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL ERROR: {ex.Message}");
                _TestResults.Add(new TestResult("Fatal Error", false, 0, ex.Message));
            }
            finally
            {
                // Stop test webserver
                StopTestWebserver();

                overallStopwatch.Stop();

                // Display summary
                DisplayTestSummary(overallStopwatch.ElapsedMilliseconds);
            }
        }

        #region Test-Categories

        private static async Task RunStandardHttpTests()
        {
            Console.WriteLine("=== Standard HTTP Tests ===");

            var httpSites = new[]
            {
                "http://httpbin.org/get",
                "http://example.com",
                _WebserverUrl + "/test"
            };

            foreach (string url in httpSites)
            {
                await RunTest($"HTTP GET: {url}", async () =>
                {
                    using var req = new RestRequest(url);
                    using var resp = await req.SendAsync();

                    if (resp.StatusCode != 200)
                        throw new Exception($"Expected 200, got {resp.StatusCode}");

                    if (resp.ContentLength == null || resp.ContentLength <= 0)
                        throw new Exception("No content received");

                    string data = resp.DataAsString;
                    if (string.IsNullOrEmpty(data))
                        throw new Exception("Empty response data");
                });
            }
        }

        private static async Task RunStandardHttpsTests()
        {
            Console.WriteLine("\n=== Standard HTTPS Tests ===");

            var httpsSites = new[]
            {
                "https://httpbin.org/get",
                "https://www.google.com",
                "https://www.github.com",
                "https://api.github.com"
            };

            foreach (string url in httpsSites)
            {
                await RunTest($"HTTPS GET: {url}", async () =>
                {
                    using var req = new RestRequest(url);
                    using var resp = await req.SendAsync();

                    if (resp.StatusCode < 200 || resp.StatusCode >= 400)
                        throw new Exception($"HTTP error: {resp.StatusCode}");

                    // Handle different response types
                    if (resp.ChunkedTransferEncoding)
                    {
                        // For chunked responses, just verify we can read at least one chunk
                        var chunk = await resp.ReadChunkAsync();
                        if (chunk == null || chunk.Data == null || chunk.Data.Length == 0)
                            throw new Exception("No chunk data received");
                    }
                    else if (resp.ServerSentEvents)
                    {
                        // For SSE responses, just verify we can read at least one event
                        var evt = await resp.ReadEventAsync();
                        if (evt == null)
                            throw new Exception("No SSE data received");
                    }
                    else
                    {
                        // For regular responses, check content
                        string data = resp.DataAsString;
                        if (string.IsNullOrEmpty(data) && resp.ContentLength > 0)
                            throw new Exception("Empty response data despite content length > 0");
                    }
                });
            }
        }

        private static async Task RunLocalHttpTests()
        {
            Console.WriteLine("\n=== Local HTTP Tests ===");

            // Test HEAD method
            await RunTest("HEAD method", async () =>
            {
                using var req = new RestRequest(_WebserverUrl + "/methods", HttpMethod.Head);
                using var resp = await req.SendAsync();

                if (resp.StatusCode != 200)
                    throw new Exception($"Expected 200, got {resp.StatusCode}");

                // Verify the response was handled correctly
                if (resp.Headers == null)
                    throw new Exception("HEAD response should have headers");
            });

            // Test GET method
            await RunTest("GET method", async () =>
            {
                using var req = new RestRequest(_WebserverUrl + "/methods", HttpMethod.Get);
                using var resp = await req.SendAsync();

                if (resp.StatusCode != 200)
                    throw new Exception($"Expected 200, got {resp.StatusCode}");

                string responseData = resp.DataAsString;
                if (string.IsNullOrEmpty(responseData))
                    throw new Exception("GET response should have body content");

                // Parse JSON response and verify method
                var jsonDoc = JsonDocument.Parse(responseData);
                string method = jsonDoc.RootElement.GetProperty("method").GetString()!;
                if (method != "GET")
                    throw new Exception($"Expected method 'GET', got '{method}'");

                // Verify path
                string path = jsonDoc.RootElement.GetProperty("path").GetString()!;
                if (path != "/methods")
                    throw new Exception($"Expected path '/methods', got '{path}'");
            });

            // Test POST method with JSON payload
            await RunTest("POST method", async () =>
            {
                using var req = new RestRequest(_WebserverUrl + "/methods", HttpMethod.Post);
                req.ContentType = "application/json";
                string postData = "{\"test\": \"data\", \"number\": 123}";

                using var resp = await req.SendAsync(postData);

                if (resp.StatusCode != 200)
                    throw new Exception($"Expected 200, got {resp.StatusCode}");

                string responseData = resp.DataAsString;
                var jsonDoc = JsonDocument.Parse(responseData);
                string method = jsonDoc.RootElement.GetProperty("method").GetString()!;
                string body = jsonDoc.RootElement.GetProperty("body").GetString()!;

                if (method != "POST")
                    throw new Exception($"Expected method 'POST', got '{method}'");
                if (body != postData)
                    throw new Exception($"Expected body '{postData}', got '{body}'");
            });

            // Test PUT method with JSON payload
            await RunTest("PUT method", async () =>
            {
                using var req = new RestRequest(_WebserverUrl + "/methods", HttpMethod.Put);
                req.ContentType = "application/json";
                string putData = "{\"update\": \"value\", \"id\": 456}";

                using var resp = await req.SendAsync(putData);

                if (resp.StatusCode != 200)
                    throw new Exception($"Expected 200, got {resp.StatusCode}");

                string responseData = resp.DataAsString;
                var jsonDoc = JsonDocument.Parse(responseData);
                string method = jsonDoc.RootElement.GetProperty("method").GetString()!;
                string body = jsonDoc.RootElement.GetProperty("body").GetString()!;

                if (method != "PUT")
                    throw new Exception($"Expected method 'PUT', got '{method}'");
                if (body != putData)
                    throw new Exception($"Expected body '{putData}', got '{body}'");
            });

            // Test DELETE method
            await RunTest("DELETE method", async () =>
            {
                using var req = new RestRequest(_WebserverUrl + "/methods", HttpMethod.Delete);
                using var resp = await req.SendAsync();

                if (resp.StatusCode != 200)
                    throw new Exception($"Expected 200, got {resp.StatusCode}");

                string responseData = resp.DataAsString;
                var jsonDoc = JsonDocument.Parse(responseData);
                string method = jsonDoc.RootElement.GetProperty("method").GetString()!;

                if (method != "DELETE")
                    throw new Exception($"Expected method 'DELETE', got '{method}'");

                // Verify body is empty for DELETE
                string body = jsonDoc.RootElement.GetProperty("body").GetString()!;
                if (!string.IsNullOrEmpty(body))
                    throw new Exception($"Expected empty body for DELETE, got '{body}'");
            });

            // Test OPTIONS method
            await RunTest("OPTIONS method", async () =>
            {
                using var req = new RestRequest(_WebserverUrl + "/methods", HttpMethod.Options);
                using var resp = await req.SendAsync();

                if (resp.StatusCode != 200)
                    throw new Exception($"Expected 200, got {resp.StatusCode}");

                string responseData = resp.DataAsString;
                var jsonDoc = JsonDocument.Parse(responseData);
                string method = jsonDoc.RootElement.GetProperty("method").GetString()!;

                if (method != "OPTIONS")
                    throw new Exception($"Expected method 'OPTIONS', got '{method}'");
            });

            // Test PATCH method with JSON payload
            await RunTest("PATCH method", async () =>
            {
                using var req = new RestRequest(_WebserverUrl + "/methods", new HttpMethod("PATCH"));
                req.ContentType = "application/json";
                string patchData = "{\"patch\": \"operation\", \"field\": \"value\"}";

                using var resp = await req.SendAsync(patchData);

                if (resp.StatusCode != 200)
                    throw new Exception($"Expected 200, got {resp.StatusCode}");

                string responseData = resp.DataAsString;
                var jsonDoc = JsonDocument.Parse(responseData);
                string method = jsonDoc.RootElement.GetProperty("method").GetString()!;
                string body = jsonDoc.RootElement.GetProperty("body").GetString()!;

                if (method != "PATCH")
                    throw new Exception($"Expected method 'PATCH', got '{method}'");
                if (body != patchData)
                    throw new Exception($"Expected body '{patchData}', got '{body}'");
            });

            // Test with custom headers
            await RunTest("Custom headers", async () =>
            {
                using var req = new RestRequest(_WebserverUrl + "/methods", HttpMethod.Get);
                req.Headers.Add("X-Custom-Header", "test-value");
                req.Headers.Add("X-Another-Header", "another-value");

                using var resp = await req.SendAsync();

                if (resp.StatusCode != 200)
                    throw new Exception($"Expected 200, got {resp.StatusCode}");

                // Verify we can read the response and method is correct
                string responseData = resp.DataAsString;
                var jsonDoc = JsonDocument.Parse(responseData);
                string method = jsonDoc.RootElement.GetProperty("method").GetString()!;

                if (method != "GET")
                    throw new Exception($"Expected method 'GET', got '{method}'");
            });

            // Test with URL parameters
            await RunTest("URL parameters", async () =>
            {
                using var req = new RestRequest(_WebserverUrl + "/methods?param1=value1&param2=value2", HttpMethod.Get);
                using var resp = await req.SendAsync();

                if (resp.StatusCode != 200)
                    throw new Exception($"Expected 200, got {resp.StatusCode}");

                string responseData = resp.DataAsString;
                var jsonDoc = JsonDocument.Parse(responseData);
                string method = jsonDoc.RootElement.GetProperty("method").GetString()!;

                if (method != "GET")
                    throw new Exception($"Expected method 'GET', got '{method}'");
            });
        }


        private static async Task RunChunkedTransferTests()
        {
            Console.WriteLine("\n=== Chunked Transfer Tests ===");

            await RunTest("Chunked Transfer Encoding", async () =>
            {
                using var req = new RestRequest(_WebserverUrl + "/chunked", HttpMethod.Post);
                req.ChunkedTransfer = true;

                // Send chunks to server
                for (int i = 0; i < 5; i++)
                {
                    await req.SendChunkAsync($"Client chunk {i}\n", false);
                }

                // Get response
                using var resp = await req.SendChunkAsync(Array.Empty<byte>(), true);

                if (!resp.ChunkedTransferEncoding)
                    throw new Exception("Response not marked as chunked");

                var chunks = new List<string>();
                int chunkCount = 0;

                Console.Write("\n    Receiving chunks: ");

                while (true)
                {
                    var chunk = await resp.ReadChunkAsync();
                    if (chunk == null)
                        break;

                    chunkCount++;
                    Console.Write($"{chunkCount} ");

                    if (chunk.Data != null && chunk.Data.Length > 0)
                    {
                        string chunkText = Encoding.UTF8.GetString(chunk.Data);
                        chunks.Add(chunkText);

                        // Validate chunk content matches expected format
                        // Note: Watson Webserver sends "Server chunk N\n" but ReadChunkAsync() using ReadLineAsync()
                        // strips the \n since it's used as the line delimiter in the chunked protocol
                        int expectedChunkIndex = chunkCount - 1;
                        string expectedContent = $"Server chunk {expectedChunkIndex}";
                        if (chunkText != expectedContent)
                        {
                            throw new Exception($"Chunk {chunkCount} content mismatch. Expected: '{expectedContent}', Got: '{chunkText}'");
                        }
                    }

                    if (chunk.IsFinal)
                        break;
                }

                Console.Write($"(total: {chunkCount})");

                if (chunkCount == 0)
                    throw new Exception("No chunks received");

                if (chunks.Count != 20)
                    throw new Exception($"Expected exactly 20 chunks, got {chunks.Count}");

                // Final validation: ensure all chunks were received in correct order
                for (int i = 0; i < chunks.Count; i++)
                {
                    string expectedContent = $"Server chunk {i}";
                    if (chunks[i] != expectedContent)
                    {
                        throw new Exception($"Chunk {i} validation failed. Expected: '{expectedContent}', Got: '{chunks[i]}'");
                    }
                }
            });
        }

        private static async Task RunServerSentEventTests()
        {
            Console.WriteLine("\n=== Server-Sent Events Tests ===");

            await RunTest("Server-Sent Events", async () =>
            {
                using var req = new RestRequest(_WebserverUrl + "/sse");
                using var resp = await req.SendAsync();

                if (!resp.ServerSentEvents)
                    throw new Exception("Response not marked as SSE");

                var events = new List<string>();
                int eventCount = 0;

                Console.Write("\n    Receiving events: ");

                while (true)
                {
                    var evt = await resp.ReadEventAsync();
                    if (evt == null)
                        break;

                    eventCount++;
                    Console.Write($"{eventCount} ");

                    if (!string.IsNullOrEmpty(evt.Data))
                    {
                        // Trim whitespace from SSE data (Watson may add \r characters)
                        string cleanEventData = evt.Data.Trim();
                        events.Add(cleanEventData);

                        // Validate event content matches expected format
                        string expectedContent;
                        if (eventCount <= 20)
                        {
                            expectedContent = $"Event data {eventCount - 1}";
                        }
                        else
                        {
                            expectedContent = "Final event";
                        }

                        if (cleanEventData != expectedContent)
                        {
                            // Debug: show byte-by-byte comparison
                            var expectedBytes = Encoding.UTF8.GetBytes(expectedContent);
                            var actualBytes = Encoding.UTF8.GetBytes(cleanEventData);

                            string expectedHex = BitConverter.ToString(expectedBytes).Replace("-", "");
                            string actualHex = BitConverter.ToString(actualBytes).Replace("-", "");

                            throw new Exception($"Event {eventCount} content mismatch.\nExpected ({expectedBytes.Length} bytes): '{expectedContent}'\nExpected hex: {expectedHex}\nGot ({actualBytes.Length} bytes): '{evt.Data}'\nActual hex: {actualHex}");
                        }
                    }

                    if (eventCount >= 21) // Expect 20 events + 1 final event
                        break;
                }

                Console.Write($"(total: {eventCount})");

                if (eventCount == 0)
                    throw new Exception("No events received");

                if (events.Count != 21)
                    throw new Exception($"Expected exactly 21 events (20 + final), got {events.Count}");

                // Final validation: ensure all events were received in correct order
                for (int i = 0; i < events.Count - 1; i++)
                {
                    string expectedContent = $"Event data {i}";
                    if (events[i] != expectedContent)
                    {
                        throw new Exception($"Event {i} validation failed. Expected: '{expectedContent}', Got: '{events[i]}'");
                    }
                }

                // Validate final event
                if (events[events.Count - 1] != "Final event")
                {
                    throw new Exception($"Final event validation failed. Expected: 'Final event', Got: '{events[events.Count - 1]}'");
                }
            });
        }

        private static async Task RunSerializationTests()
        {
            Console.WriteLine("\n=== Serialization Tests ===");

            await RunTest("JSON Serialization", async () =>
            {
                var testObject = new TestData { Name = "Test", Value = 42 };

                using var req = new RestRequest(_WebserverUrl + "/echo", HttpMethod.Post);
                req.ContentType = "application/json";

                string json = JsonSerializer.Serialize(testObject);
                using var resp = await req.SendAsync(json);

                if (resp.StatusCode != 200)
                    throw new Exception($"Expected 200, got {resp.StatusCode}");

                var result = resp.DataFromJson<TestData>();
                if (result == null || result.Name != testObject.Name || result.Value != testObject.Value)
                    throw new Exception("JSON deserialization failed");
            });

            await RunTest("Custom Serializer", async () =>
            {
                var testObject = new TestData { Name = "Custom", Value = 99 };

                using var req = new RestRequest(_WebserverUrl + "/echo", HttpMethod.Post);
                req.ContentType = "application/json";

                var customSerializer = new CustomSerializer();
                string json = customSerializer.SerializeJson(testObject);
                using var resp = await req.SendAsync(json);
                resp.SerializationHelper = customSerializer;

                var result = resp.DataFromJson<TestData>();
                if (result == null || result.Name != testObject.Name || result.Value != testObject.Value)
                    throw new Exception("Custom serialization failed");
            });
        }

        private static async Task RunDataPropertyTests()
        {
            Console.WriteLine("\n=== Data Property Tests ===");

            await RunTest("DataAsString Property", async () =>
            {
                using var req = new RestRequest(_WebserverUrl + "/text");
                using var resp = await req.SendAsync();

                string dataAsString = resp.DataAsString;
                if (string.IsNullOrEmpty(dataAsString))
                    throw new Exception("DataAsString is empty");

                // Verify calling it again returns the same data
                string dataAsString2 = resp.DataAsString;
                if (dataAsString != dataAsString2)
                    throw new Exception("DataAsString not consistent");
            });

            await RunTest("DataAsBytes Property", async () =>
            {
                using var req = new RestRequest(_WebserverUrl + "/text");
                using var resp = await req.SendAsync();

                byte[] dataAsBytes = resp.DataAsBytes;
                if (dataAsBytes == null || dataAsBytes.Length == 0)
                    throw new Exception("DataAsBytes is empty");

                string converted = Encoding.UTF8.GetString(dataAsBytes);
                string dataAsString = resp.DataAsString;

                if (converted != dataAsString)
                    throw new Exception("DataAsBytes and DataAsString don't match");
            });

            await RunTest("Data Stream Property", async () =>
            {
                using var req = new RestRequest(_WebserverUrl + "/text");
                using var resp = await req.SendAsync();

                var stream = resp.Data;
                if (stream == null)
                    throw new Exception("Data stream is null");

                if (!stream.CanRead)
                    throw new Exception("Data stream is not readable");
            });
        }

        private static async Task RunCancellationTests()
        {
            Console.WriteLine("\n=== Cancellation Tests ===");

            await RunTest("Cancellation Token Timeout", async () =>
            {
                using var cts = new CancellationTokenSource();
                cts.CancelAfter(100); // Cancel after 100ms

                try
                {
                    using var req = new RestRequest(_WebserverUrl + "/delay");
                    using var resp = await req.SendAsync(cts.Token);

                    throw new Exception("Request should have been cancelled");
                }
                catch (OperationCanceledException)
                {
                    // Expected behavior
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unexpected exception type: {ex.GetType().Name}");
                }
            });

            await RunTest("Cancellation During Chunk Reading", async () =>
            {
                using var cts = new CancellationTokenSource();

                using var req = new RestRequest(_WebserverUrl + "/chunked-slow", HttpMethod.Post);
                req.ChunkedTransfer = true;

                using var resp = await req.SendChunkAsync(Array.Empty<byte>(), true);

                try
                {
                    // Start reading chunks, then cancel after first chunk
                    var chunk1 = await resp.ReadChunkAsync();
                    if (chunk1 == null)
                        throw new Exception("Expected at least one chunk");

                    cts.CancelAfter(50); // Cancel quickly

                    var chunk2 = await resp.ReadChunkAsync(cts.Token);
                    // May or may not throw depending on timing
                }
                catch (OperationCanceledException)
                {
                    // Expected behavior
                }
            });
        }

        private static async Task RunNegativeTests()
        {
            Console.WriteLine("\n=== Negative Tests ===");

            await RunTest("Invalid URL", async () =>
            {
                try
                {
                    using var req = new RestRequest("invalid-url");
                    using var resp = await req.SendAsync();
                    throw new Exception("Should have thrown an exception");
                }
                catch (Exception ex) when (!(ex.Message == "Should have thrown an exception"))
                {
                    // Expected - any exception except our test exception
                }
            });

            await RunTest("404 Not Found", async () =>
            {
                using var req = new RestRequest(_WebserverUrl + "/nonexistent");
                using var resp = await req.SendAsync();

                if (resp.StatusCode != 404)
                    throw new Exception($"Expected 404, got {resp.StatusCode}");
            });

            await RunTest("SSE/Chunked Property Conflicts", async () =>
            {
                using var req = new RestRequest(_WebserverUrl + "/text");
                using var resp = await req.SendAsync();

                // These should not throw since it's a regular response
                var data = resp.Data;
                string dataStr = resp.DataAsString;
                byte[] dataBytes = resp.DataAsBytes;

                if (data == null || string.IsNullOrEmpty(dataStr) || dataBytes == null)
                    throw new Exception("Regular response properties should work");
            });
        }

        #endregion

        #region Test-Infrastructure

        private static async Task RunTest(string testName, Func<Task> testAction)
        {
            Console.Write($"  {testName}...");

            var stopwatch = Stopwatch.StartNew();
            bool passed = false;
            string? error = null;

            try
            {
                await testAction();
                passed = true;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($" PASS ({stopwatch.ElapsedMilliseconds}ms)");
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" FAIL ({stopwatch.ElapsedMilliseconds}ms) - {ex.Message}");
            }
            finally
            {
                Console.ResetColor();
                stopwatch.Stop();
                _TestResults.Add(new TestResult(testName, passed, stopwatch.ElapsedMilliseconds, error));
            }
        }

        private static async Task StartTestWebserver()
        {
            try
            {
                var settings = new WebserverSettings("localhost", 9000, false);
                _Webserver = new Webserver(settings, DefaultRoute);
                _Webserver.Start();

                Console.WriteLine("Test webserver started on " + _WebserverUrl);
                await Task.Delay(500); // Give server time to start
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to start test webserver: {ex.Message}");
            }
        }

        private static void StopTestWebserver()
        {
            try
            {
                _Webserver?.Stop();
                _Webserver?.Dispose();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        private static async Task DefaultRoute(HttpContextBase ctx)
        {
            string path = ctx.Request.Url.RawWithoutQuery;

            try
            {
                switch (path)
                {
                    case "/test":
                        ctx.Response.StatusCode = 200;
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send("Hello from test server!");
                        break;

                    case "/text":
                        ctx.Response.StatusCode = 200;
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send("Sample text response for testing");
                        break;

                    case "/echo":
                        ctx.Response.StatusCode = 200;
                        ctx.Response.ContentType = ctx.Request.ContentType;
                        await ctx.Response.Send(ctx.Request.DataAsString);
                        break;

                    case "/chunked":
                        ctx.Response.ChunkedTransfer = true;
                        ctx.Response.StatusCode = 200;
                        ctx.Response.ContentType = "text/plain";

                        for (int i = 0; i < 20; i++)
                        {
                            byte[] data = Encoding.UTF8.GetBytes($"Server chunk {i}\n");
                            await ctx.Response.SendChunk(data, false);
                            await Task.Delay(250);
                        }
                        await ctx.Response.SendChunk(Array.Empty<byte>(), true);
                        break;

                    case "/chunked-slow":
                        ctx.Response.ChunkedTransfer = true;
                        ctx.Response.StatusCode = 200;
                        ctx.Response.ContentType = "text/plain";

                        for (int i = 0; i < 5; i++)
                        {
                            byte[] data = Encoding.UTF8.GetBytes($"Slow chunk {i}\n");
                            await ctx.Response.SendChunk(data, false);
                            await Task.Delay(1000); // Longer delay for cancellation testing
                        }
                        await ctx.Response.SendChunk(Array.Empty<byte>(), true);
                        break;

                    case "/sse":
                        ctx.Response.StatusCode = 200;
                        ctx.Response.ServerSentEvents = true;

                        for (int i = 0; i < 20; i++)
                        {
                            await ctx.Response.SendEvent($"Event data {i}", false);
                            await Task.Delay(250);
                        }
                        await ctx.Response.SendEvent("Final event", true);
                        break;

                    case "/delay":
                        // Long delay for cancellation testing
                        await Task.Delay(2000);
                        ctx.Response.StatusCode = 200;
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send("Delayed response");
                        break;

                    case "/methods":
                        // Handle different HTTP methods
                        var method = ctx.Request.Method;
                        ctx.Response.StatusCode = 200;
                        ctx.Response.ContentType = "application/json";

                        var response = new
                        {
                            method = method.ToString(),
                            path = path,
                            body = ctx.Request.DataAsString,
                            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                        };

                        string jsonResponse = JsonSerializer.Serialize(response);
                        await ctx.Response.Send(jsonResponse);
                        break;

                    default:
                        ctx.Response.StatusCode = 404;
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send("Not found");
                        break;
                }
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 500;
                ctx.Response.ContentType = "text/plain";
                await ctx.Response.Send($"Server error: {ex.Message}");
            }
        }

        private static void DisplayTestSummary(long totalTimeMs)
        {
            Console.WriteLine();
            Console.WriteLine("=====================================");
            Console.WriteLine("TEST SUMMARY");
            Console.WriteLine("=====================================");

            int passed = 0;
            int failed = 0;
            long totalTestTime = 0;

            foreach (var result in _TestResults)
            {
                totalTestTime += result.ElapsedMs;

                if (result.Passed)
                {
                    passed++;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ {result.TestName} ({result.ElapsedMs}ms)");
                }
                else
                {
                    failed++;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"✗ {result.TestName} ({result.ElapsedMs}ms) - {result.Error}");
                }
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.WriteLine($"Total Tests: {_TestResults.Count}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Passed: {passed}");
            Console.ResetColor();

            if (failed > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed: {failed}");
                Console.ResetColor();
            }

            Console.WriteLine($"Total Test Time: {totalTestTime}ms");
            Console.WriteLine($"Total Execution Time: {totalTimeMs}ms");

            if (failed == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nALL TESTS PASSED! 🎉");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n{failed} TEST(S) FAILED ❌");
            }
            Console.ResetColor();
        }

        #endregion

        #region Supporting-Classes

        public class TestResult
        {
            public string TestName { get; set; } = "";
            public bool Passed { get; set; }
            public long ElapsedMs { get; set; }
            public string? Error { get; set; }

            public TestResult(string testName, bool passed, long elapsedMs, string? error = null)
            {
                TestName = testName;
                Passed = passed;
                ElapsedMs = elapsedMs;
                Error = error;
            }
        }

        public class TestData
        {
            public string Name { get; set; } = "";
            public int Value { get; set; }
        }

        public class CustomSerializer : RestWrapperSerializationHelper
        {
            public T DeserializeJson<T>(string json) where T : class, new()
            {
                return JsonSerializer.Deserialize<T>(json) ?? new T();
            }

            public string SerializeJson(object obj, bool pretty = true)
            {
                var options = new JsonSerializerOptions();
                if (pretty)
                    options.WriteIndented = true;
                return JsonSerializer.Serialize(obj, options);
            }
        }

        #endregion

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}