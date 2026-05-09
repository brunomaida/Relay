<#
.SYNOPSIS
    Parses a .trx test result file and reports per-test duration grouped by threshold.

.DESCRIPTION
    Reads the latest TRX file from tests/Relay.Tests/TestResults/ (or a specified path),
    sorts tests by duration, and groups them into buckets: >5min, >2min, >1min, >30s, <=30s.

.PARAMETER TrxPath
    Explicit path to a .trx file. If omitted, the latest file in TestResults/ is used.

.PARAMETER RunFirst
    Run the test suite (excluding Stress/Perf/Endurance) and generate a fresh TRX, then parse it.

.PARAMETER Filter
    dotnet test --filter expression used when -RunFirst is specified.
    Default: "Category!=Endurance&Category!=Stress&Category!=Perf"

.PARAMETER Thresholds
    Comma-separated threshold values in seconds for grouping.
    Default: 300,120,60,30

.EXAMPLE
    .\tools\test-timing.ps1
    .\tools\test-timing.ps1 -RunFirst
    .\tools\test-timing.ps1 -TrxPath .\tests\Relay.Tests\TestResults\results.trx
    .\tools\test-timing.ps1 -RunFirst -Filter "Category=Stress"
    .\tools\test-timing.ps1 -Thresholds 60,120,300
#>

param(
    [string]   $TrxPath,
    [switch]   $RunFirst,
    [string]   $Filter     = "Category!=Endurance&Category!=Stress&Category!=Perf",
    [int[]]    $Thresholds = @(300, 120, 60, 30)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$TrxDir = Join-Path $PSScriptRoot "..\tests\Relay.Tests\TestResults"

if ($RunFirst) {
    Write-Host "Running tests with filter: $Filter" -ForegroundColor Cyan
    $trxFile = Join-Path $TrxDir "timing-$(Get-Date -Format 'yyyyMMdd-HHmmss').trx"
    dotnet test Relay.sln -c Release --no-build `
        --filter $Filter `
        --logger "trx;LogFileName=$trxFile" | Out-Null
    $TrxPath = $trxFile
    Write-Host "TRX written: $TrxPath" -ForegroundColor Green
}

if (-not $TrxPath) {
    if (-not (Test-Path $TrxDir)) {
        Write-Error "TestResults directory not found: $TrxDir. Run with -RunFirst to generate."
    }
    $latest = Get-ChildItem $TrxDir -Recurse -Filter "*.trx" |
              Sort-Object LastWriteTime -Descending |
              Select-Object -First 1
    if (-not $latest) {
        Write-Error "No .trx files found in $TrxDir. Run with -RunFirst to generate."
    }
    $TrxPath = $latest.FullName
    Write-Host "Using: $TrxPath" -ForegroundColor DarkGray
}

[xml]$trx = Get-Content $TrxPath -Encoding UTF8

$ns = @{ mstest = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010" }

$results = $trx.SelectNodes("//mstest:UnitTestResult", `
    ([System.Xml.XmlNamespaceManager]::new($trx.NameTable) | ForEach-Object {
        $_.AddNamespace("mstest", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"); $_
    })) | ForEach-Object {
    $dur = [TimeSpan]::Zero
    if ($_.duration) { $dur = [TimeSpan]::Parse($_.duration) }
    [PSCustomObject]@{
        Test     = $_.testName -replace "^.*?\.", ""   # strip namespace prefix
        Outcome  = $_.outcome
        Duration = $dur
        Seconds  = $dur.TotalSeconds
    }
} | Sort-Object Seconds -Descending

$thresholdsSorted = ($Thresholds | Sort-Object -Descending)
$labels = @{
    300 = "> 5 min"
    120 = "> 2 min"
     60 = "> 1 min"
     30 = "> 30 s"
}

$total      = $results.Count
$totalSecs  = ($results | Measure-Object Seconds -Sum).Sum

Write-Host ""
Write-Host ("=" * 80)
Write-Host " TEST TIMING REPORT — $([System.IO.Path]::GetFileName($TrxPath))"
Write-Host " Tests: $total   Wall-clock sum: $([TimeSpan]::FromSeconds($totalSecs).ToString('hh\:mm\:ss\.fff'))"
Write-Host ("=" * 80)

$printed = [System.Collections.Generic.HashSet[string]]::new()

foreach ($threshold in $thresholdsSorted) {
    $label    = if ($labels.ContainsKey($threshold)) { $labels[$threshold] } else { "> ${threshold}s" }
    $group    = $results | Where-Object { $_.Seconds -gt $threshold -and -not $printed.Contains($_.Test) }
    if ($group.Count -eq 0) { continue }

    Write-Host ""
    Write-Host " [$label]" -ForegroundColor Yellow
    Write-Host ("-" * 80)

    foreach ($r in $group) {
        $outcomeColor = switch ($r.Outcome) {
            "Passed"  { "Green"  }
            "Failed"  { "Red"    }
            "Skipped" { "DarkGray" }
            default   { "White"  }
        }
        $durStr = $r.Duration.ToString("mm\:ss\.fff")
        Write-Host ("  {0,-60} {1,9}  " -f ($r.Test -replace "^.*?\.", ""), $durStr) -NoNewline
        Write-Host $r.Outcome -ForegroundColor $outcomeColor
        [void]$printed.Add($r.Test)
    }
}

$fastCount = $results | Where-Object { $_.Seconds -le ($thresholdsSorted | Select-Object -Last 1) } | Measure-Object | Select-Object -ExpandProperty Count
Write-Host ""
Write-Host " [<= $($thresholdsSorted | Select-Object -Last 1) s]  $fastCount tests (not listed)" -ForegroundColor DarkGray
Write-Host ("=" * 80)
Write-Host ""
