# RestWrapper

[![][nuget-img]][nuget]

[nuget]:     https://www.nuget.org/packages/RestWrapper/
[nuget-img]: https://badge.fury.io/nu/Object.svg

A simple C# class library to help simplify REST API requests and responses (RESTful HTTP and HTTPS)

## New in v2.1.2

- Fix misnamed content-length parameter

## Test Apps

Test projects are included which will help you exercise the class library.
 
## Examples

```
//
// simple GET example
//

using RestWrapper;
using System.IO;

RestRequest req = new RestRequest(
	"http://www.google.com/",
	HttpMethod.GET,
	null,                     // Dictionary<string, string> headers
	null);                    // Content type

RestResponse resp = req.Send();
Console.WriteLine("Status : " + resp.StatusCode);
// response data is in resp.Data
```

```
//
// simple POST examples
//

using RestWrapper;
using System.IO;

RestRequest req = new RestRequest(
	"http://127.0.0.1:8000/api",
	HttpMethod.POST,
	null,                         // Dictionary<string, string> headers
	"text/plain");                // Content type

string reqString = "Hello, world!";
byte[] reqBytes = Encoding.UTF8.GetBytes(reqString);
MemoryStream reqStream = new MemoryStream(reqBytes);
reqStream.Seek(0, SeekOrigin.Begin);

RestResponse resp = req.Send(reqData);
resp = req.Send(reqBytes);
resp = req.Send(reqBytes.Length, reqStream);

Console.WriteLine("Status : " + resp.StatusCode);
// response data is in resp.Data
```

```
//
// async methods
//

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
