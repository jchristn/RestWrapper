# Change Log

## Current Version

v2.3.x

- Remove Newtonsoft.JSON dependency, now leveraging ```System.Text.Json``` by default
- Add support for implementing your own deserializer

## Previous Versions

v2.2.1

- RestResponse ```DataAsBytes``` and ```DataAsString``` properties

v2.2.0

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


