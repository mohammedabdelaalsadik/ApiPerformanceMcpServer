# Execution Plan

This document describes how to build, run, verify, and demonstrate the `ApiPerformanceMcpServer` project.

## 1. Prerequisites

- .NET SDK that can target `net9.0`
- An MCP-compatible client
- Internet access for live API tests

## 2. Build

From the repository root:

```powershell
dotnet build .\ApiPerformanceMcpServer
```

Expected result:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

## 3. Run Manually

```powershell
dotnet run --project .\ApiPerformanceMcpServer
```

The server uses MCP stdio transport. Manual execution starts the process and waits for MCP protocol messages. For normal usage, configure an MCP client to launch the server.

## 4. MCP Client Configuration

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

After adding the configuration, restart the MCP client and confirm these tools are available:

- `test_api`
- `compare_apis`

## 5. Test One API

Prompt:

```text
Test https://httpbin.org/get performance with 5 iterations.
```

Tool arguments:

```json
{
  "url": "https://httpbin.org/get",
  "method": "GET",
  "iterations": 5
}
```

## 6. Compare Two APIs

Prompt:

```text
Compare https://jsonplaceholder.typicode.com/posts and https://httpbin.org/get 10 times and tell me which is faster.
```

Tool arguments:

```json
{
  "api1Url": "https://jsonplaceholder.typicode.com/posts",
  "api2Url": "https://httpbin.org/get",
  "method": "GET",
  "iterations": 10
}
```

## 7. Validate Error Handling

Invalid URL:

```json
{
  "url": "not-a-url",
  "method": "GET",
  "iterations": 3
}
```

Unsupported method:

```json
{
  "url": "https://httpbin.org/get",
  "method": "PUT",
  "iterations": 3
}
```

Expected behavior: the tool returns structured JSON with `failureCount` and an `errors` array.

## 8. Generate Execution Report

From the reports folder:

```powershell
cd .\ApiPerformanceMcpServer\Reports
powershell -ExecutionPolicy Bypass -File .\GenerateReportPdf.ps1
```

Output:

```text
ApiPerformanceExecutionReport.pdf
```

The report includes build verification, MCP tool discovery, performance statistics, charts, observations, and recommendation text.

## 9. Recommended Demo Flow

1. Show the repository structure.
2. Build the project.
3. Open the MCP client and confirm `test_api` and `compare_apis` are discovered.
4. Run `test_api` against `https://httpbin.org/get`.
5. Run `compare_apis` against the two sample APIs.
6. Open the generated PDF report.
7. Explain that failures, timeouts, and non-success status codes are returned in structured JSON.
