# RestWrapper

[![][nuget-img]][nuget]

[nuget]:     https://www.nuget.org/packages/RestWrapper/
[nuget-img]: https://badge.fury.io/nu/Object.svg

A simple C# class library to help simplify REST API requests and responses (RESTful HTTP and HTTPS)

## New in v2.0.1

- Breaking changes, major refactor
- Support for streams (in addition to byte arrays)

## Test App

Two test projects are included which will help you exercise the class library, one using byte arrays for input/output data, and the other for streams.
 
## Example
```
using RestWrapper;
using System.IO;

RestRequest req = null;

// simple GET using byte array for response data

req = new RestRequest(
	"http://www.google.com/",
	HttpMethod.GET,
	null, 						// Dictionary<string, string> headers
	null,						// Content type
	true						// Read response data into Data
	);
resp = req.Send(null);
Console.WriteLine("Status : " + resp.StatusCode);
Console.WriteLine("Data   : " + Encoding.UTF8.GetString(resp.Data));

// simple POST using byte array for request and response data

req = new RestRequest(
	"http://127.0.0.1:8000/api",
	HttpMethod.POST,
	null, 						// Dictionary<string, string> headers
	"text/plain",				// Content type
	true						// Read response data into Data
	);
byte[] data = Encoding.UTF8.GetBytes("Hello, world!");
resp = req.Send(data);
Console.WriteLine("Status : " + resp.StatusCode);
Console.WriteLine("Data   : " + Encoding.UTF8.GetString(resp.Data));

// simple POST using input and output stream

req = new RestRequest(
	"http://127.0.0.1:8000/api",
	HttpMethod.POST,
	null, 						// Dictionary<string, string> headers
	"text/plain",				// Content type
	false						// Read response data into Data
	);
byte[] data = Encoding.UTF8.GetBytes("Hello, world!");
MemoryStream ms = new MemoryStream(data);
resp = req.Send(ms, data.Length);
Console.WriteLine("Status : " + resp.StatusCode);
// response data is in resp.DataStream
```