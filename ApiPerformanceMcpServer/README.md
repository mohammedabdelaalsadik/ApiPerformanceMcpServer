# ApiPerformanceMcpServer

A .NET 9 console MCP server that exposes tools for testing and comparing API performance.

## Tools

- `test_api`: calls one API multiple times and returns status codes plus average, minimum, maximum, and total elapsed time.
- `compare_apis`: calls two APIs multiple times and recommends the faster API.

Sample APIs:

- `https://jsonplaceholder.typicode.com/posts`
- `https://httpbin.org/get`

## Run

From PowerShell:

```powershell
cd D:\Teaching\Projects\MCP_Test_Performance
dotnet build .\ApiPerformanceMcpServer
dotnet run --project .\ApiPerformanceMcpServer
```

The server uses MCP stdio transport, so it is meant to be launched by an MCP client rather than opened in a browser.

For a full build, run, verification, and demo workflow, see [docs/EXECUTION_PLAN.md](docs/EXECUTION_PLAN.md).

## Execution Steps

1. Open PowerShell.

2. Go to the workspace:

```powershell
cd D:\Teaching\Projects\MCP_Test_Performance
```

3. Build the project:

```powershell
dotnet build .\ApiPerformanceMcpServer
```

4. Run the server:

```powershell
dotnet run --project .\ApiPerformanceMcpServer
```

5. Stop the server with `Ctrl+C`.

For normal MCP usage, configure your MCP client to launch this command automatically. Because this is a stdio MCP server, running it manually only starts the process and waits for MCP protocol messages.

## Connect From An MCP Client

Add this server to your MCP client configuration:

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

- Compare these two APIs 10 times and tell me which is faster.
- Test this API performance with 20 iterations.
- Compare API A and API B and show average, min, and max response time.

## Sample Tool Calls

`test_api`:

```json
{
  "url": "https://httpbin.org/get",
  "method": "GET",
  "iterations": 5
}
```

`compare_apis`:

```json
{
  "api1Url": "https://jsonplaceholder.typicode.com/posts",
  "api2Url": "https://httpbin.org/get",
  "method": "GET",
  "iterations": 10
}
```

## Sample JSON Result

```json
{
  "api1": {
    "url": "https://jsonplaceholder.typicode.com/posts",
    "statusCodes": [200, 200, 200, 200, 200],
    "successCount": 5,
    "failureCount": 0,
    "averageResponseMs": 92.45,
    "minResponseMs": 81.27,
    "maxResponseMs": 118.66,
    "totalElapsedMs": 463.04,
    "errors": []
  },
  "api2": {
    "url": "https://httpbin.org/get",
    "statusCodes": [200, 200, 200, 200, 200],
    "successCount": 5,
    "failureCount": 0,
    "averageResponseMs": 147.32,
    "minResponseMs": 130.91,
    "maxResponseMs": 178.22,
    "totalElapsedMs": 737.11,
    "errors": []
  },
  "fasterApi": "https://jsonplaceholder.typicode.com/posts",
  "differenceMs": 54.87,
  "recommendation": "Use https://jsonplaceholder.typicode.com/posts for lower average latency. It was faster by 54.87 ms. Both APIs had the same failure count."
}
```

## Error Handling

The tools return errors in the `errors` array for:

- invalid URLs
- unsupported HTTP methods
- request timeout after 30 seconds
- non-success HTTP status codes
- exception messages from failed requests

## Reports And Sharing

- Execution report PDF: [Reports/ApiPerformanceExecutionReport.pdf](Reports/ApiPerformanceExecutionReport.pdf)
- Report source HTML: [Reports/ApiPerformanceExecutionReport.html](Reports/ApiPerformanceExecutionReport.html)
- LinkedIn post draft: [docs/LINKEDIN_POST.md](docs/LINKEDIN_POST.md)
