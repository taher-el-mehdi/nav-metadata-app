#Requires -Version 5.1
<#
.SYNOPSIS
  Publishes NAV Metadata (self-contained win-x64), creates a portable ZIP, and builds the Inno Setup installer.

.PARAMETER SkipInstaller
  Only publish and zip; skip Inno Setup when ISCC.exe is unavailable.

.EXAMPLE
  .\scripts\build-release.ps1
  .\scripts\build-release.ps1 -SkipInstaller
#>
param(
    [switch]$SkipInstaller
)

$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

$csproj = Join-Path $Root 'NAVMetadata.UI.csproj'
if (-not (Test-Path $csproj)) {
    throw "Project file not found: $csproj"
}

$versionMatch = Select-String -Path $csproj -Pattern '<Version>([^<]+)</Version>' | Select-Object -First 1
if (-not $versionMatch) {
    throw 'Could not read <Version> from NAVMetadata.UI.csproj'
}
$Version = $versionMatch.Matches[0].Groups[1].Value
$InstallerFileName = "NAVMetadata-Setup-$Version.exe"

$publishDir = Join-Path $Root 'artifacts\publish\win-x64'
$installerDir = Join-Path $Root 'artifacts\installer'
$zipPath = Join-Path $Root "artifacts\NAVMetadata-v$Version-win-x64.zip"

Write-Host "==> NAV Metadata release build v$Version" -ForegroundColor Cyan

if (Test-Path (Join-Path $Root 'artifacts')) {
    Remove-Item (Join-Path $Root 'artifacts\*') -Recurse -Force -ErrorAction SilentlyContinue
}
New-Item -ItemType Directory -Force -Path $publishDir, $installerDir | Out-Null

Write-Host '==> dotnet publish (self-contained win-x64)...' -ForegroundColor Cyan
dotnet publish $csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishReadyToRun=true `
    -o $publishDir
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host '==> Creating portable ZIP...' -ForegroundColor Cyan
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $zipPath

$isccCandidates = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
    (Get-Command ISCC.exe -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source)
) | Where-Object { $_ -and (Test-Path $_) }

$iscc = $isccCandidates | Select-Object -First 1
$iss = Join-Path $Root 'installer\NAVMetadata.iss'

if ($SkipInstaller) {
    Write-Host '==> Skipping Inno Setup (-SkipInstaller).' -ForegroundColor Yellow
} elseif (-not $iscc) {
    Write-Host '==> Inno Setup not found. Install from https://jrsoftware.org/isinfo.php' -ForegroundColor Yellow
    Write-Host '    Publish output and ZIP are ready in artifacts\' -ForegroundColor Yellow
} else {
    Write-Host "==> Building installer with $iscc ..." -ForegroundColor Cyan
    & $iscc $iss "/DMyAppVersion=$Version"
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host ''
Write-Host 'Release artifacts:' -ForegroundColor Green
Write-Host "  ZIP:       $zipPath"
if (Test-Path (Join-Path $installerDir $InstallerFileName)) {
    Write-Host "  Installer: $(Join-Path $installerDir $InstallerFileName)"
}
Write-Host ''
Write-Host 'Upload to GitHub Releases:' -ForegroundColor Green
Write-Host "  1. $InstallerFileName  (primary download)"
Write-Host "  2. NAVMetadata-v$Version-win-x64.zip (portable)"
