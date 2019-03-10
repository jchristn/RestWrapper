# RestWrapper

[![][nuget-img]][nuget]

[nuget]:     https://www.nuget.org/packages/RestWrapper/
[nuget-img]: https://badge.fury.io/nu/Object.svg

A simple C# class library to help simplify REST API requests and responses (RESTful HTTP and HTTPS)

As of v1.0.9, RestWrapper now targets both .NET Core 2.0 and .NET Framework 4.5.2.

## Test App

A test project is included which will help you exercise the class library.

## Available APIs

Two static methods exist: SendRequest and SendRequestSafe.  The differences are as follows:
- SendRequest will throw any exception encountered to the caller
- SendRequestSafe will take any WebException and create a RestResponse object from it.  Other exceptions are thrown to the caller

## Example
```
using RestWrapper;

//
// Simple GET with No Credentials
//
RestResponse resp = RestRequest.SendRequest(
	"http://www.github.com/",	// URL
	null, 						// content-type
	"GET",						// verb/method
	null, null, false, 			// user, password, encode
	null, 						// headers
	null);						// byte array data

//
// Enumerate response
//
Console.WriteLine(
	"Received " + resp.StatusCode + " (" + resp.ContentLength + " bytes) " +
	"with data: " + Encoding.UTF8.GetString(resp.Data));

//
// POST with Headers and Credentials
//
Dictionary<string, string> headers = new Dictionary<string, string>();
headers.Add("x-custom-header", "my-custom-value");
byte[] data = Encoding.UTF8.GetBytes("some-field=some-value&hello=world");

RestResponse resp = RestRequest.SendRequest(
	"https://my.server.com/form",			// URL
	"application/x-www-form-urlencoded",	// content-type
	"POST",									// verb/method
	"my-username", "my-password", true, 	// user, password, encode
	headers,								// headers
	data);									// byte array data

//
// Enumerate response
//
Console.WriteLine(resp.ToString());		// Easy peasy
```