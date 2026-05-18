# Change Log

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


