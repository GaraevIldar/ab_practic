<#
.SYNOPSIS
  Запускает unit-тесты с измерением покрытия и генерирует HTML-отчёт.
#>

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$testProject = Join-Path $root "tests\PracticalWork.Library.UnitTests\PracticalWork.Library.UnitTests.csproj"
$settings = Join-Path $root "tests\PracticalWork.Library.UnitTests\coverlet.runsettings"
$resultsDir = Join-Path $root "tests\TestResults"
$reportDir = Join-Path $root "tests\CoverageReport"

if (Test-Path $resultsDir) { Remove-Item $resultsDir -Recurse -Force }
if (Test-Path $reportDir) { Remove-Item $reportDir -Recurse -Force }

Write-Host "[1/3] Running tests with coverage..." -ForegroundColor Cyan
dotnet test $testProject `
    --collect:"XPlat Code Coverage" `
    --settings $settings `
    --results-directory $resultsDir `
    --logger "console;verbosity=normal"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "[2/3] Installing reportgenerator (if needed)..." -ForegroundColor Cyan
dotnet tool install -g dotnet-reportgenerator-globaltool 2>$null

Write-Host "[3/3] Generating HTML report..." -ForegroundColor Cyan
$cobertura = Get-ChildItem -Path $resultsDir -Recurse -Filter "coverage.cobertura.xml" | Select-Object -First 1
if (-not $cobertura) {
    Write-Host "Coverage file not found." -ForegroundColor Red
    exit 1
}

reportgenerator `
    "-reports:$($cobertura.FullName)" `
    "-targetdir:$reportDir" `
    "-reporttypes:Html;TextSummary"

$summary = Join-Path $reportDir "Summary.txt"
if (Test-Path $summary) {
    Write-Host "`n=== Coverage Summary ===" -ForegroundColor Green
    Get-Content $summary
}

Write-Host "`nHTML report: $(Join-Path $reportDir 'index.html')" -ForegroundColor Green
