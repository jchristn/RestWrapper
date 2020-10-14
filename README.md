# RestWrapper

[![NuGet Version](https://img.shields.io/nuget/v/RestWrapper.svg?style=flat)](https://www.nuget.org/packages/RestWrapper/) [![NuGet](https://img.shields.io/nuget/dt/RestWrapper.svg)](https://www.nuget.org/packages/RestWrapper) 

A simple C# class library to help simplify REST API requests and responses (RESTful HTTP and HTTPS)

## New in v2.1.4

- ToString() method on RestRequest
- Retarget to support .NET Standard 2.0, .NET Core 2.0, and .NET Framework 4.5.1

## Test Apps

Test projects are included which will help you exercise the class library.
 
## Examples

```csharp
// simple GET example
using RestWrapper;
using System.IO;

RestRequest req = new RestRequest(
	"http://www.google.com/",
	HttpMethod.GET,
	null,                     // Dictionary<string, string> headers
	null);                    // Content type

RestResponse resp = req.Send();
Console.WriteLine("Status: " + resp.StatusCode);
// response data is in resp.Data
```

```csharp
// simple POST example
using RestWrapper;
using System.IO;

RestRequest req = new RestRequest(
	"http://127.0.0.1:8000/api",
	HttpMethod.POST,
	null,                         // Dictionary<string, string> headers
	"text/plain");                // Content type

string reqString = "Hello, world!";
RestResponse resp = req.Send(reqData);
Console.WriteLine("Status : " + resp.StatusCode);
// response data is in resp.Data
```

```csharp
// async methods
using RestWrapper;
using System.IO;
using System.Threading.Tasks;

RestRequest req = new RestRequest(
	"http://127.0.0.1:8000/api",
	HttpMethod.POST,
	null,                         // Dictionary<string, string> headers
	"text/plain");                // Content type

RestResponse resp = await req.SendAsync("Hello, world!");
Console.WriteLine("Status : " + resp.StatusCode);
// response data is in resp.Data
```

## Version History

Please refer to CHANGELOG.md for version history.
