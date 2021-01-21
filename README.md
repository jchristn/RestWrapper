![alt tag](https://raw.githubusercontent.com/jchristn/RestWrapper/master/assets/icon.ico)

# RestWrapper

[![NuGet Version](https://img.shields.io/nuget/v/RestWrapper.svg?style=flat)](https://www.nuget.org/packages/RestWrapper/) [![NuGet](https://img.shields.io/nuget/dt/RestWrapper.svg)](https://www.nuget.org/packages/RestWrapper) 

A simple C# class library to help simplify sending REST API requests and retrieving responses (RESTful HTTP and HTTPS)

## New in v2.2.1

- RestResponse ```DataAsBytes``` and ```DataAsString``` properties; ```DataFromJson<T>``` method

## Test Apps

Test projects are included which will help you exercise the class library.
 
## Examples

```csharp
// simple GET example
using RestWrapper;
using System.IO;

RestRequest req = new RestRequest("http://www.google.com/");
RestResponse resp = req.Send();
Console.WriteLine("Status: " + resp.StatusCode);
// response data is in resp.Data
```

```csharp
// simple POST example
using RestWrapper;
using System.IO;

RestRequest req = new RestRequest("http://127.0.0.1:8000/api", HttpMethod.POST);
RestResponse resp = req.Send("Hello, world!");
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
	"text/plain"); // Content type

RestResponse resp = await req.SendAsync("Hello, world!");
Console.WriteLine("Status : " + resp.StatusCode);
// response data is in resp.Data
```

```csharp
// sending form data
using RestWrapper;

RestRequest req = new RestRequest("http://127.0.0.1:8000/api", HttpMethod.POST);

Dictionary<string, string> form = new Dictionary<string, string>();
form.Add("foo", "bar");
form.Add("hello", "world how are you");

RestResponse resp = req.Send(form);
Console.WriteLine("Status : " + resp.StatusCode);
```

```csharp
// deserializing JSON
using RestWrapper;

RestRequest req = new RestRequest("http://127.0.0.1:8000/api");
RestResponse resp = req.Send();
MyObject obj = resp.DataFromJson<MyObject>();
```

## Version History

Please refer to CHANGELOG.md for version history.
