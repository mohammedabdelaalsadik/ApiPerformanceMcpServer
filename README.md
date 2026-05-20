# ApiPerformanceMcpServer

A .NET 9 console MCP server for testing and comparing API performance.

The server exposes MCP tools that can be called from an MCP-compatible client to measure one API endpoint or compare two API endpoints using repeated HTTP requests.

## Features

- .NET 9 and C#
- ModelContextProtocol SDK
- MCP stdio transport
- `HttpClientFactory`
- `System.Diagnostics.Stopwatch`
- Structured JSON output
- Clean project structure
- Error handling for invalid URLs, unsupported methods, timeouts, non-success HTTP status codes, and request exceptions
- PDF and HTML execution reports with charts

## MCP Tools

| Tool | Description |
| --- | --- |
| `test_api` | Tests one API endpoint multiple times and returns status codes, success/failure count, average/min/max response time, and total elapsed time. |
| `compare_apis` | Tests two API endpoints and recommends the faster API based on average response time. |

## Project Location

Source code is under:

```text
ApiPerformanceMcpServer/
```

Key files:

- [Program.cs](ApiPerformanceMcpServer/Program.cs)
- [Tools/ApiPerformanceTools.cs](ApiPerformanceMcpServer/Tools/ApiPerformanceTools.cs)
- [Services/ApiPerformanceService.cs](ApiPerformanceMcpServer/Services/ApiPerformanceService.cs)
- [Models](ApiPerformanceMcpServer/Models)

## Quick Start

Build:

```powershell
dotnet build .\ApiPerformanceMcpServer
```

Run:

```powershell
dotnet run --project .\ApiPerformanceMcpServer
```

This is an MCP stdio server. It is normally launched by an MCP client rather than opened in a browser.

## MCP Client Configuration

```json
{
  "mcpServers": {
    "api-performance": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "D:\\Teaching\\Projects\\MCP_Test_Performance\\ApiPerformanceMcpServer"
      ]
    }
  }
}
```

## Sample Prompts

```text
Test https://httpbin.org/get performance with 5 iterations.
```

```text
Compare https://jsonplaceholder.typicode.com/posts and https://httpbin.org/get 10 times and tell me which is faster.
```

```text
Compare API A and API B and show average, min, and max response time.
```

## Documentation

- [Detailed README](ApiPerformanceMcpServer/README.md)
- [Execution Plan - Markdown](ApiPerformanceMcpServer/docs/EXECUTION_PLAN.md)
- [Execution Plan - HTML](ApiPerformanceMcpServer/docs/EXECUTION_PLAN.html)
- [LinkedIn Post Draft](ApiPerformanceMcpServer/docs/LINKEDIN_POST.md)

## Reports

- [Execution Report PDF](ApiPerformanceMcpServer/Reports/ApiPerformanceExecutionReport.pdf)
- [Execution Report HTML](ApiPerformanceMcpServer/Reports/ApiPerformanceExecutionReport.html)
- [PDF Generator Script](ApiPerformanceMcpServer/Reports/GenerateReportPdf.ps1)

## Sample Result

```json
{
  "url": "https://httpbin.org/get",
  "statusCodes": [200, 200],
  "successCount": 2,
  "failureCount": 0,
  "averageResponseMs": 735.85,
  "minResponseMs": 726.81,
  "maxResponseMs": 744.89,
  "totalElapsedMs": 1476.68,
  "errors": []
}
```

## Repository

```text
https://github.com/mohammedabdelaalsadik/ApiPerformanceMcpServer
```
