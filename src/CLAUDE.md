# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RestWrapper is a C# library that simplifies sending REST API requests and retrieving responses. It supports both HTTP and HTTPS with RESTful patterns and provides a simple, disposable wrapper around .NET's HTTP client functionality.

## Architecture

### Core Components

- **RestRequest** (`RestWrapper/RestRequest.cs`): Main class for creating and configuring HTTP requests. Implements IDisposable and supports all standard HTTP methods.
- **RestResponse** (`RestWrapper/RestResponse.cs`): Represents the response from HTTP requests. Contains response data, headers, status codes, and timing information.
- **SerializationHelper** (`RestWrapper/DefaultSerializationHelper.cs`, `RestWrapper/ISerializationHelper.cs`): Handles JSON serialization/deserialization using System.Text.Json by default, with extensibility through ISerializationHelper interface.

### Additional Features

- **Server-Sent Events**: `ServerSentEvent.cs` and `ServerSentEventReader.cs` provide SSE support
- **Chunked Transfer**: `ChunkData.cs`, `ChunkedContent.cs`, `ChunkedStreamReader.cs`, and `ChunkSender.cs` handle chunked transfer encoding
- **Content Type Parsing**: `ContentTypeParser.cs` handles content-type header parsing including charset detection
- **Authorization**: `AuthorizationHeader.cs` provides authorization header utilities

## Build and Development Commands

### Building the Solution
```bash
dotnet build RestWrapper.sln
```

### Building Specific Projects
```bash
dotnet build RestWrapper/RestWrapper.csproj      # Main library
dotnet build Test/Test.csproj                    # Interactive test application
```

### Running Tests
The solution includes several test projects that can be run individually:

```bash
dotnet run --project Test/Test.csproj                          # Interactive test app
dotnet run --project TestStream/TestStream.csproj              # Stream testing
dotnet run --project TestSerializer/TestSerializer.csproj      # Serialization testing
dotnet run --project TestCancellation/TestCancellation.csproj  # Cancellation testing
dotnet run --project TestChunkedTransfer/TestChunkedTransfer.csproj  # Chunked transfer testing
dotnet run --project TestServerSentEvents/TestServerSentEvents.csproj  # Server-sent events testing
```

### Package Creation
The main RestWrapper project is configured to generate NuGet packages on build (`GeneratePackageOnBuild=true`).

## Target Frameworks

The library supports multiple .NET versions:
- .NET Standard 2.0 and 2.1 (for broad compatibility)
- .NET Framework 4.6.2 and 4.8
- .NET 6.0 and 8.0

Test projects target: .NET Framework 4.6.2, 4.8, .NET 6.0, and 8.0

## Key Dependencies

- **System.Net.Http** (4.3.4): Core HTTP functionality
- **System.Text.Json** (8.0.5): JSON serialization
- **Timestamps** (1.0.11): Timing utilities for performance tracking

## Code Style and Implementation Rules

**These rules must be followed STRICTLY for consistency and maintainability:**

### File Structure and Organization
- Namespace declaration should always be at the top, with using statements contained INSIDE the namespace block
- All Microsoft and standard system library usings should be first, in alphabetical order, followed by other using statements, in alphabetical order
- Limit each file to containing exactly one class or exactly one enum - do not nest multiple classes or enums in a single file

### Documentation
- All public members, constructors, and public methods must have code documentation
- No code documentation should be applied to private members or private methods
- Document nullability in XML comments
- Document thread safety guarantees in XML comments
- Document which exceptions public methods can throw using /// <exception> tags
- Where appropriate, ensure code documentation outlines default values, minimum values, and maximum values, specifying what different values mean or effects they may have

### Naming and Variables
- Private class member variable names must start with an underscore and then be Pascal cased (i.e. `_FooBar`, not `_fooBar`)
- Do not use `var` when defining a variable - use its actual type
- All public members should have explicit getters and setters using backing variables when value requires range or null validation

### Async Programming
- All async methods follow the `...Async()` naming pattern
- Async calls should use `.ConfigureAwait(false)` where appropriate
- Every async method should accept a CancellationToken as an input property, unless the class has a CancellationToken as a class member or a CancellationTokenSource as a class member
- Async calls should check whether or not cancellation has been requested at appropriate places
- When implementing a method that returns an IEnumerable, also create an async variant that includes a CancellationToken

### Error Handling and Exceptions
- Use specific exception types rather than generic Exception
- Always include meaningful error messages with context
- Consider using custom exception types for domain-specific errors
- Use exception filters when appropriate: `catch (SqlException ex) when (ex.Number == 2601)`

### Resource Management
- Implement IDisposable/IAsyncDisposable when holding unmanaged resources or disposable objects
- Use 'using' statements or 'using' declarations for IDisposable objects
- Follow the full Dispose pattern with `protected virtual void Dispose(bool disposing)`
- Always call `base.Dispose()` in derived classes
- The library uses disposable patterns extensively - wrap RestRequest and RestResponse objects in using statements

### Null Safety and Validation
- Use nullable reference types (enable `<Nullable>enable</Nullable>` in project files)
- Validate input parameters with guard clauses at method start
- Use `ArgumentNullException.ThrowIfNull()` for .NET 6+ or manual null checks
- Consider using the Result pattern or Option/Maybe types for methods that can fail
- Proactively identify and eliminate any situations in code where null might cause exceptions to be thrown

### Threading and Concurrency
- Use Interlocked operations for simple atomic operations
- Prefer ReaderWriterLockSlim over lock for read-heavy scenarios

### LINQ and Collections
- Prefer LINQ methods over manual loops when readability is not compromised
- Use `.Any()` instead of `.Count() > 0` for existence checks
- Be aware of multiple enumeration issues - consider `.ToList()` when needed
- Use `.FirstOrDefault()` with null checks rather than `.First()` when element might not exist

### General Guidelines
- Do not use tuples unless absolutely, absolutely necessary
- Avoid using constant values for things that a developer may later want to configure - instead use a public member with a backing private member set to a reasonable default
- Do not make assumptions about opaque class members or methods - ask for implementation details when needed
- If manually prepared SQL strings exist, assume there is a good reason for it
- Always compile code and ensure it is free of errors and warnings

## Development Notes

- Performance timing is built-in via the `RestResponse.Time` property
- Default serialization uses System.Text.Json but can be customized via ISerializationHelper
- For localhost testing, prefer `127.0.0.1` over `localhost` to avoid IPv6 connection delays