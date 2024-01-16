![alt tag](https://raw.githubusercontent.com/jchristn/RestWrapper/master/assets/icon.ico)

# RestWrapper

[![NuGet Version](https://img.shields.io/nuget/v/RestWrapper.svg?style=flat)](https://www.nuget.org/packages/RestWrapper/) [![NuGet](https://img.shields.io/nuget/dt/RestWrapper.svg)](https://www.nuget.org/packages/RestWrapper) 

A simple C# class library to help simplify sending REST API requests and retrieving responses (RESTful HTTP and HTTPS)

## Special Thanks

Thanks go out to the community for their help in making this library great!

@nhaberl @jmkinzer @msasanmh @lanwah @nhaberl 

## New in v3.0.x

- Minor breaking changes
- Migration from ```HttpWebRequest``` to ```HttpClient```
- Strong naming
- Retrieve query elements from ```RestRequest.Query``` property

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

## A Note on Performance and 'localhost'

RestWrapper uses the underlying ```HttpWebRequest``` and ```HttpWebResponse``` classes from ```System.Net```.  When using ```localhost``` as the target URL, you may notice in Wireshark that ```HttpWebRequest``` will first attempt to connect to the IPv6 loopback address, and not all services listen on IPv6.  **This can create a material delay of more than 1 second**.  In these cases, it is recommended that you use ```127.0.0.1``` instead of ```localhost``` for these cases.

## Basic Telemetry

The ```RestResponse``` object contains a property called ```Time``` that can be useful for understanding how long a request took to complete.

```csharp
RestRequest req = new RestRequest("https://www.cnn.com");
RestResponse resp = req.Send();
Console.WriteLine("Start    : " + resp.Time.Start);
Console.WriteLine("End      : " + resp.Time.End);
Console.WriteLine("Total ms : " + resp.Time.TotalMs + "ms");
```

## Deserializing Response Data

The method ```RestResponse.DataFromJson<T>()``` will deserialize using ```System.Text.Json```.  You can override the ```RestResponse.SerializationHelper``` property with your own implementation of ```ISerializationHelper``` if you wish to use your own deserializer.  Thank you @nhaberl for the suggestion.

## Version History

Please refer to CHANGELOG.md for version history.
