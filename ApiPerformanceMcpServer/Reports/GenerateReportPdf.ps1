$ErrorActionPreference = 'Stop'

$reportDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$pdfPath = Join-Path $reportDir 'ApiPerformanceExecutionReport.pdf'

function Escape-PdfText {
    param([string]$Text)
    return ($Text -replace '\\', '\\' -replace '\(', '\(' -replace '\)', '\)')
}

function N {
    param([double]$Value)
    return $Value.ToString('0.##', [Globalization.CultureInfo]::InvariantCulture)
}

function Add-Text {
    param(
        [System.Text.StringBuilder]$Builder,
        [double]$X,
        [double]$Y,
        [double]$Size,
        [string]$Text,
        [switch]$Bold
    )

    $font = if ($Bold) { '/F2' } else { '/F1' }
    [void]$Builder.AppendLine("BT $font $(N $Size) Tf $(N $X) $(N $Y) Td ($(Escape-PdfText $Text)) Tj ET")
}

function Add-Line {
    param(
        [System.Text.StringBuilder]$Builder,
        [double]$X1,
        [double]$Y1,
        [double]$X2,
        [double]$Y2,
        [double]$Width = 1
    )

    [void]$Builder.AppendLine("0.58 0.64 0.72 RG $(N $Width) w $(N $X1) $(N $Y1) m $(N $X2) $(N $Y2) l S")
}

function Add-Rect {
    param(
        [System.Text.StringBuilder]$Builder,
        [double]$X,
        [double]$Y,
        [double]$W,
        [double]$H,
        [string]$Rgb
    )

    [void]$Builder.AppendLine("$Rgb rg $(N $X) $(N $Y) $(N $W) $(N $H) re f")
}

function Add-Box {
    param(
        [System.Text.StringBuilder]$Builder,
        [double]$X,
        [double]$Y,
        [double]$W,
        [double]$H,
        [string]$Rgb = '0.97 0.98 0.99'
    )

    [void]$Builder.AppendLine("$Rgb rg $(N $X) $(N $Y) $(N $W) $(N $H) re f")
    [void]$Builder.AppendLine("0.84 0.87 0.91 RG 0.8 w $(N $X) $(N $Y) $(N $W) $(N $H) re S")
}

function Add-SectionTitle {
    param([System.Text.StringBuilder]$Builder, [double]$Y, [string]$Title)
    Add-Text $Builder 40 $Y 15 $Title -Bold
    Add-Line $Builder 40 ($Y - 7) 555 ($Y - 7) 1.2
}

function Add-Metric {
    param([System.Text.StringBuilder]$Builder, [double]$X, [double]$Y, [string]$Value, [string]$Label)
    Add-Box $Builder $X $Y 160 58
    Add-Rect $Builder $X $Y 5 58 '0.13 0.40 0.67'
    Add-Text $Builder ($X + 14) ($Y + 32) 18 $Value -Bold
    Add-Text $Builder ($X + 14) ($Y + 15) 9 $Label
}

function Add-TableRow {
    param(
        [System.Text.StringBuilder]$Builder,
        [double]$X,
        [double]$Y,
        [double[]]$Widths,
        [string[]]$Values,
        [switch]$Header
    )

    $height = 24
    $currentX = $X
    for ($i = 0; $i -lt $Widths.Count; $i++) {
        $fill = if ($Header) { '0.93 0.95 0.97' } else { '1 1 1' }
        Add-Box $Builder $currentX $Y $Widths[$i] $height $fill
        Add-Text $Builder ($currentX + 5) ($Y + 9) 8.3 $Values[$i] -Bold:$Header
        $currentX += $Widths[$i]
    }
}

function Add-BarChart {
    param(
        [System.Text.StringBuilder]$Builder,
        [double]$X,
        [double]$Y,
        [string]$Title,
        [string[]]$Labels,
        [double[]]$Values,
        [double]$Max,
        [string[]]$Colors
    )

    Add-Box $Builder $X $Y 245 165 '1 1 1'
    Add-Text $Builder ($X + 12) ($Y + 145) 11 $Title -Bold
    Add-Line $Builder ($X + 38) ($Y + 35) ($X + 38) ($Y + 130)
    Add-Line $Builder ($X + 38) ($Y + 35) ($X + 225) ($Y + 35)
    Add-Text $Builder ($X + 38) ($Y + 18) 7 '0 ms'
    Add-Text $Builder ($X + 8) ($Y + 128) 7 "$([int]$Max) ms"

    $barWidth = 48
    for ($i = 0; $i -lt $Values.Count; $i++) {
        $barHeight = [Math]::Max(1, ($Values[$i] / $Max) * 92)
        $barX = $X + 65 + ($i * 86)
        $barY = $Y + 35
        Add-Rect $Builder $barX $barY $barWidth $barHeight $Colors[$i]
        Add-Text $Builder $barX ($barY + $barHeight + 8) 8 "$(N $Values[$i])"
        Add-Text $Builder ($barX - 4) ($Y + 18) 7 $Labels[$i]
    }
}

$page1 = [System.Text.StringBuilder]::new()
Add-Text $page1 40 795 24 'API Performance MCP Server - Execution Report' -Bold
Add-Text $page1 40 775 9 'Project: ApiPerformanceMcpServer | Runtime: .NET 9 | Transport: MCP stdio | Report date: May 20, 2026'

Add-SectionTitle $page1 745 'Executive Summary'
Add-Text $page1 40 723 10 'The MCP server was built and verified successfully. It initialized over MCP, exposed the expected'
Add-Text $page1 40 709 10 'tools, and executed live API performance tests using the configured sample endpoints.'
Add-Metric $page1 40 632 '0' 'Build warnings and errors'
Add-Metric $page1 218 632 '2' 'MCP tools discovered'
Add-Metric $page1 396 632 '115.26 ms' 'Latency advantage for httpbin'

Add-SectionTitle $page1 595 'Build And MCP Verification'
Add-TableRow $page1 40 555 @(150,365) @('Check','Result') -Header
Add-TableRow $page1 40 531 @(150,365) @('dotnet build','Build succeeded with 0 warnings and 0 errors.')
Add-TableRow $page1 40 507 @(150,365) @('MCP initialize','Server initialized as ApiPerformanceMcpServer version 1.0.0.0.')
Add-TableRow $page1 40 483 @(150,365) @('MCP tools/list','Discovered tools: compare_apis, test_api.')
Add-TableRow $page1 40 459 @(150,365) @('Runtime fix applied','Default stdout logging was disabled to keep MCP stdio protocol-safe.')

Add-SectionTitle $page1 420 'test_api Result'
Add-Text $page1 40 399 9 'Endpoint tested: https://httpbin.org/get | Method: GET | Iterations: 2'
Add-TableRow $page1 40 360 @(72,58,58,76,62,62,62) @('Status','Success','Failure','Average','Min','Max','Total') -Header
Add-TableRow $page1 40 336 @(72,58,58,76,62,62,62) @('200, 200','2','0','735.85 ms','726.81','744.89','1476.68')

Add-SectionTitle $page1 300 'compare_apis Result'
Add-TableRow $page1 40 260 @(152,58,58,58,62,62,62) @('API','Status','Success','Failure','Average','Min','Max') -Header
Add-TableRow $page1 40 236 @(152,58,58,58,62,62,62) @('jsonplaceholder/posts','200','1','1','323.61','323.61','323.61')
Add-TableRow $page1 40 212 @(152,58,58,58,62,62,62) @('httpbin/get','200,200','2','0','208.35','170.83','245.87')
Add-Box $page1 40 146 515 42 '0.93 0.99 0.96'
Add-Rect $page1 40 146 5 42 '0.06 0.46 0.43'
Add-Text $page1 52 171 10 'Recommendation' -Bold
Add-Text $page1 52 156 9 'Use https://httpbin.org/get for lower average latency. It was faster by 115.26 ms and had fewer failures.'

$page2 = [System.Text.StringBuilder]::new()
Add-Text $page2 40 795 20 'Execution Charts' -Bold
Add-BarChart $page2 40 590 'Average Response Time' @('test_api','httpbin') @(735.85,208.35) 800 @('0.13 0.40 0.67','0.06 0.46 0.43')
Add-BarChart $page2 310 590 'Comparison Average Latency' @('API 1','API 2') @(323.61,208.35) 350 @('0.71 0.33 0.04','0.06 0.46 0.43')
Add-BarChart $page2 40 390 'Success Count' @('test_api','httpbin') @(2,2) 2 @('0.06 0.46 0.43','0.06 0.46 0.43')
Add-BarChart $page2 310 390 'Failure Count' @('API 1','API 2') @(1,0) 2 @('0.86 0.15 0.15','0.06 0.46 0.43')

Add-SectionTitle $page2 340 'Observations'
Add-Text $page2 40 318 10 '1. The MCP server exposed both required tools and returned structured JSON output.'
Add-Text $page2 40 301 10 '2. httpbin.org/get completed both comparison iterations successfully and had the lower average latency.'
Add-Text $page2 40 284 10 '3. jsonplaceholder.typicode.com/posts recorded one SSL connection failure during the comparison run.'
Add-Text $page2 40 267 10 '4. A larger iteration count is recommended before using these numbers for production decisions.'

Add-Box $page2 40 205 515 46 '1 0.97 0.93'
Add-Rect $page2 40 205 5 46 '0.71 0.33 0.04'
Add-Text $page2 52 231 10 'Note' -Bold
Add-Text $page2 52 216 9 'The comparison run includes a reported SSL error for API 1. This confirms error handling is visible in final output.'

Add-Text $page2 40 50 8 'Generated from verified MCP execution results for ApiPerformanceMcpServer.'

$content1 = $page1.ToString()
$content2 = $page2.ToString()

$objects = New-Object System.Collections.Generic.List[string]
$objects.Add('<< /Type /Catalog /Pages 2 0 R >>')
$objects.Add('<< /Type /Pages /Kids [3 0 R 5 0 R] /Count 2 >>')
$objects.Add('<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 7 0 R /F2 8 0 R >> >> /Contents 4 0 R >>')
$objects.Add("<< /Length $([Text.Encoding]::ASCII.GetByteCount($content1)) >>`nstream`n$content1`nendstream")
$objects.Add('<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 7 0 R /F2 8 0 R >> >> /Contents 6 0 R >>')
$objects.Add("<< /Length $([Text.Encoding]::ASCII.GetByteCount($content2)) >>`nstream`n$content2`nendstream")
$objects.Add('<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>')
$objects.Add('<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>')

$encoding = [Text.Encoding]::ASCII
$pdf = [System.Text.StringBuilder]::new()
$offsets = New-Object System.Collections.Generic.List[int]
[void]$pdf.AppendLine('%PDF-1.4')
[void]$pdf.AppendLine('% MCP API performance report')

for ($i = 0; $i -lt $objects.Count; $i++) {
    $offsets.Add($encoding.GetByteCount($pdf.ToString()))
    [void]$pdf.AppendLine("$($i + 1) 0 obj")
    [void]$pdf.AppendLine($objects[$i])
    [void]$pdf.AppendLine('endobj')
}

$xrefOffset = $encoding.GetByteCount($pdf.ToString())
[void]$pdf.AppendLine('xref')
[void]$pdf.AppendLine("0 $($objects.Count + 1)")
[void]$pdf.AppendLine('0000000000 65535 f ')
foreach ($offset in $offsets) {
    [void]$pdf.AppendLine($offset.ToString('0000000000') + ' 00000 n ')
}
[void]$pdf.AppendLine('trailer')
[void]$pdf.AppendLine("<< /Size $($objects.Count + 1) /Root 1 0 R >>")
[void]$pdf.AppendLine('startxref')
[void]$pdf.AppendLine($xrefOffset.ToString([Globalization.CultureInfo]::InvariantCulture))
[void]$pdf.AppendLine('%%EOF')

[IO.File]::WriteAllBytes($pdfPath, $encoding.GetBytes($pdf.ToString()))
Get-Item $pdfPath | Select-Object FullName, Length, LastWriteTime
