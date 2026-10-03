# Change Log

## v3.3.2

- Dependency updates: System.Text.Json 10.0.12 and Timestamps 1.0.13
- Test dependency updates: NUnit 5.0.0, NUnit.Analyzers 4.15.0, NUnit3TestAdapter 6.3.0, Microsoft.NET.Test.Sdk 18.10.1, coverlet.collector 10.1.0, Touchstone 0.2.0, Watson 7.2.2
- All 140 shared test cases pass under the console, xUnit and NUnit runners on .NET 8.0 and 10.0; no public API changes

## v3.3.1

- `RestResponse.Headers` keeps repeated response headers as separate values, so `Headers.GetValues("Set-Cookie")` returns one entry per cookie instead of a single comma-joined string that cannot be split safely (cookie `Expires` dates contain commas)
- `Headers[name]` and `Headers.Get(name)` still return the comma-joined value, unchanged from earlier versions
- Add a `ResponseHeaders` test suite covering repeated, single, comma-containing and missing headers, both on `RestResponse` directly and through `RestRequest.SendAsync`

## v3.3.0

- Dependency updates: System.Text.Json 10.0.11 and Timestamps 1.0.12
- Expanded positive and negative test coverage; no public API changes

## v3.2.0

- Add constructor overloads that accept a caller-supplied `HttpClient`
- Keep caller-supplied clients caller-owned; `RestRequest` will not dispose them
- Apply authorization and custom headers at the request-message level so shared clients do not leak per-request state
- Reject RestWrapper-owned transport settings when a caller-supplied `HttpClient` is used
- Improve chunk-read cancellation behavior
- Normalize SSE multiline payload handling and improve SSE read cancellation behavior
- Fix `RestRequest.ToString()` header rendering
- Consolidate automated tests into Touchstone shared suites with console, xUnit, and NUnit runners
- Expand the automated surface to 120 shared test cases across internal/external client modes, streaming behaviors, parser edge cases, and helper utilities

## v3.1.x

- Minor breaking changes
- Better internal support for chunked-transfer encoding
- Remove non-async methods

## v3.0.x

- Minor breaking changes
- Migration from ```HttpWebRequest``` to ```HttpClient```
- Strong naming
- Retrieve query elements from ```RestRequest.Query``` property

## Previous Versions

v2.3.x

- Remove Newtonsoft.JSON dependency, now leveraging ```System.Text.Json``` by default
- Add support for implementing your own deserializer

v2.2.x

- RestResponse ```DataAsBytes``` and ```DataAsString``` properties
- Additional constructors
- Support for sending ```x-www-form-urlencoded``` data (```Send(Dictionary<string, string>)```)
- Dependency update

v2.1.5

- Additional constructors

v2.1.4

- ToString() method on RestRequest
- Retarget to support .NET Standard 2.0, .NET Core 2.0, and .NET Framework 4.5.1

v2.1.3

- Added RestRequest.Timeout parameter (in milliseconds)

v2.1.2

- Fix misnamed content-length parameter

v2.1.1

- XML documentation

v2.1.0

- Breaking changes
- Additional Send() methods including strings
- Better support for async operations and internally using async

v2.0.x

- Breaking changes, major refactor
- Support for streams (in addition to byte arrays)
- Added SendAsync methods for both byte arrays and streams


