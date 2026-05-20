# LinkedIn Post Draft

I built a small .NET 9 MCP server for API performance testing.

The project exposes two MCP tools:

- `test_api`: runs repeated GET or POST requests against one API and returns status codes, success/failure counts, average/min/max response time, and total elapsed time.
- `compare_apis`: runs the same test against two APIs and recommends the faster endpoint.

Tech used:

- .NET 9
- C#
- ModelContextProtocol SDK
- HttpClientFactory
- System.Diagnostics.Stopwatch
- Structured JSON output
- Clean architecture style

I also added error handling for invalid URLs, unsupported methods, timeouts, non-success HTTP status codes, and request exceptions.

The repo includes:

- Source code
- MCP client configuration example
- Sample prompts
- Execution plan
- Sample API tests
- A generated PDF execution report with charts

This was a useful exercise in turning a simple performance testing workflow into an MCP tool that can be called directly from an AI client.

Repository:
<paste GitHub repo URL here>

#dotnet #csharp #mcp #modelcontextprotocol #api #performance #softwareengineering #ai
