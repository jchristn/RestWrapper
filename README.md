![RestWrapper icon](https://raw.githubusercontent.com/jchristn/RestWrapper/master/assets/icon.ico)

# RestWrapper

[![NuGet Version](https://img.shields.io/nuget/v/RestWrapper.svg?style=flat)](https://www.nuget.org/packages/RestWrapper/) [![NuGet Downloads](https://img.shields.io/nuget/dt/RestWrapper.svg)](https://www.nuget.org/packages/RestWrapper)

RestWrapper is a small C# library for sending HTTP requests without rebuilding the same request, header, authorization, streaming, and response-handling code in every project.

It wraps `HttpClient` with a compact API for:

- standard REST verbs
- custom headers and content types
- basic, bearer, and raw authorization headers
- JSON serialization/deserialization
- response timing metadata
- server-sent events
- chunked request and response workflows
- optional caller-supplied `HttpClient` instances

## Why RestWrapper

If you want something lighter than a full API client framework but more structured than hand-rolled `HttpClient` calls, RestWrapper sits in the middle:

- request setup is explicit and easy to read
- response access is simple for text, bytes, streams, JSON, SSE, and chunked flows
- you can keep using your own `HttpClient` for DI, proxies, mTLS, custom handlers, and connection reuse
- the library stays close to the underlying HTTP model instead of inventing a large abstraction layer

## Target Frameworks

`RestWrapper` currently targets:

- `netstandard2.0`
- `netstandard2.1`
- `net462`
- `net48`
- `net6.0`
- `net8.0`
- `net10.0`

## Install

```powershell
dotnet add package RestWrapper
```

## Quick Start

Simple GET:

```csharp
using RestWrapper;

using RestRequest request = new RestRequest("https://api.example.com/status");
using RestResponse response = await request.SendAsync();

Console.WriteLine(response.StatusCode);
Console.WriteLine(response.DataAsString);
```

Simple POST:

```csharp
using RestWrapper;
using System.Net.Http;

using RestRequest request = new RestRequest("https://api.example.com/messages", HttpMethod.Post);
request.ContentType = "text/plain";

using RestResponse response = await request.SendAsync("Hello, world!");
Console.WriteLine(response.StatusCode);
Console.WriteLine(response.DataAsString);
```

JSON response handling:

```csharp
using RestWrapper;

using RestRequest request = new RestRequest("https://api.example.com/items/123");
using RestResponse response = await request.SendAsync();

MyDto dto = response.DataFromJson<MyDto>();
Console.WriteLine(dto.Name);
```

Form data:

```csharp
using RestWrapper;
using System.Net.Http;

using RestRequest request = new RestRequest("https://api.example.com/login", HttpMethod.Post);

Dictionary<string, string> form = new Dictionary<string, string>
{
    { "username", "alice" },
    { "password", "secret" }
};

using RestResponse response = await request.SendAsync(form);
Console.WriteLine(response.StatusCode);
```

## Request Configuration

### Headers and content type

```csharp
using RestWrapper;
using System.Net.Http;

using RestRequest request = new RestRequest("https://api.example.com/items", HttpMethod.Post);
request.ContentType = "application/json; charset=utf-8";
request.Headers.Add("X-Correlation-Id", Guid.NewGuid().ToString());
request.Headers.Add("X-Tenant", "east");

using RestResponse response = await request.SendAsync("{\"name\":\"demo\"}");
```

### Authorization

Basic auth:

```csharp
using RestWrapper;

using RestRequest request = new RestRequest("https://api.example.com/secure");
request.Authorization.User = "alice";
request.Authorization.Password = "secret";

using RestResponse response = await request.SendAsync();
```

Bearer auth:

```csharp
using RestWrapper;

using RestRequest request = new RestRequest("https://api.example.com/secure");
request.Authorization.BearerToken = "your-token";

using RestResponse response = await request.SendAsync();
```

Raw authorization header:

```csharp
using RestWrapper;

using RestRequest request = new RestRequest("https://api.example.com/secure");
request.Authorization.Raw = "Custom scheme-value";

using RestResponse response = await request.SendAsync();
```

### Streams

```csharp
using RestWrapper;
using System.Net.Http;
using System.Text;

byte[] payload = Encoding.UTF8.GetBytes("streamed payload");
using MemoryStream stream = new MemoryStream(payload);

using RestRequest request = new RestRequest("https://api.example.com/upload", HttpMethod.Put);
request.ContentType = "text/plain";

using RestResponse response = await request.SendAsync(payload.Length, stream);
Console.WriteLine(response.StatusCode);
```

## Response Handling

For regular responses, RestWrapper gives you multiple access patterns:

- `response.Data` for the raw stream
- `response.DataAsBytes` for the buffered byte array
- `response.DataAsString` for UTF-8 string content
- `response.DataFromJson<T>()` for JSON payloads

Example:

```csharp
using RestWrapper;

using RestRequest request = new RestRequest("https://api.example.com/items/123");
using RestResponse response = await request.SendAsync();

Console.WriteLine(response.IsSuccessStatusCode);
Console.WriteLine(response.ContentType);
Console.WriteLine(response.ContentLength);
Console.WriteLine(response.DataAsString);
```

### Timing metadata

Each `RestResponse` includes a `Time` property:

```csharp
using RestWrapper;

using RestRequest request = new RestRequest("https://api.example.com/status");
using RestResponse response = await request.SendAsync();

Console.WriteLine("Start    : " + response.Time.Start);
Console.WriteLine("End      : " + response.Time.End);
Console.WriteLine("Total ms : " + response.Time.TotalMs);
```

## Server-Sent Events

When the server returns `text/event-stream`, use `ReadEventAsync()` instead of `Data`, `DataAsString`, or `DataFromJson<T>()`.

```csharp
using RestWrapper;

using RestRequest request = new RestRequest("https://api.example.com/events");
using RestResponse response = await request.SendAsync();

while (true)
{
    ServerSentEvent evt = await response.ReadEventAsync();
    if (evt == null) break;

    Console.WriteLine($"[{evt.Event}] {evt.Data}");
}
```

`ServerSentEvent` exposes:

- `Id`
- `Event`
- `Data`
- `Retry`

## Chunked Transfer

RestWrapper supports chunked request sending and chunked response reading.

```csharp
using RestWrapper;
using System.Net.Http;

using RestRequest request = new RestRequest("https://api.example.com/chunked", HttpMethod.Post);
request.ChunkedTransfer = true;

await request.SendChunkAsync("chunk-1", false);
await request.SendChunkAsync("chunk-2", false);

using RestResponse response = await request.SendChunkAsync(Array.Empty<byte>(), true);

while (true)
{
    ChunkData chunk = await response.ReadChunkAsync();
    if (chunk == null) break;

    Console.WriteLine(System.Text.Encoding.UTF8.GetString(chunk.Data));

    if (chunk.IsFinal) break;
}
```

As with SSE, chunked responses are a specialized mode. Use `ReadChunkAsync()` instead of `Data`, `DataAsString`, `DataAsBytes`, or `DataFromJson<T>()`.

## Caller-Supplied HttpClient

If you already manage `HttpClient` instances through dependency injection or need custom handler behavior, you can provide your own `HttpClient` to `RestRequest`.

```csharp
using RestWrapper;
using System.Net.Http;

using HttpClient client = new HttpClient();
using RestRequest request = new RestRequest(
    "https://api.example.com/resource",
    HttpMethod.Get,
    client);

using RestResponse response = await request.SendAsync();
Console.WriteLine(response.StatusCode);
```

This is useful for:

- connection reuse
- proxy settings
- mTLS and client certificates
- custom message handlers
- centralized `HttpClientFactory` usage

Important behavior:

- caller-supplied clients remain caller-owned
- `RestRequest` will not dispose an external `HttpClient`
- per-request headers and authorization are applied at the request-message level so shared clients do not leak state between requests

When you supply your own `HttpClient`, transport-level settings also become caller-owned. In that mode, do not use these `RestRequest` properties:

- `TimeoutMilliseconds`
- `AllowAutoRedirect`
- `IgnoreCertificateErrors`
- `CertificateFilename`
- `CertificatePassword`

Configure those on your `HttpClient` / handler instead.

## Custom Serialization

`RestResponse.DataFromJson<T>()` uses `System.Text.Json` by default, but you can replace the serializer:

```csharp
using RestWrapper;

using RestRequest request = new RestRequest("https://api.example.com/items/123");
using RestResponse response = await request.SendAsync();
response.SerializationHelper = new MySerializer();

MyDto dto = response.DataFromJson<MyDto>();
```

`MySerializer` just needs to implement `ISerializationHelper`.

## Test Projects

The repository includes both interactive and automated test hosts:

- `src/Test` is the interactive console app
- `src/Test.Shared` contains the shared Touchstone suites
- `src/Test.Automated` is the console/CLI automation runner
- `src/Test.Xunit` exposes the shared suite through `dotnet test`
- `src/Test.Nunit` exposes the same shared suite through `dotnet test`

The current automated surface contains 120 shared cases covering:

- internal and caller-supplied `HttpClient` flows
- positive and negative transport behavior
- shared-client header/auth isolation
- SSE parsing, cancellation, and misuse guards
- chunked read behavior, cancellation, and framing
- response guards for regular vs streaming modes
- serializer and helper utilities

## Localhost Note

RestWrapper uses the platform `HttpClient` stack. When targeting `localhost`, some environments will try IPv6 loopback first. If your local service is only listening on IPv4, that can introduce a noticeable delay.

If you see this behavior, prefer `127.0.0.1` instead of `localhost`.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history.
