# Builds the ParallelSpace app in Release.
# Node/dotnet note: dotnet may not be on PATH; this uses the known install path.
$ErrorActionPreference = 'Stop'
$dotnet = 'C:\Program Files\dotnet\dotnet.exe'
if (-not (Test-Path $dotnet)) { $dotnet = 'dotnet' }

$proj = Join-Path $PSScriptRoot '..\src\ParallelSpace\ParallelSpace.csproj'
& $dotnet build $proj -c Release --nologo
Write-Host "`nBuilt: $((Resolve-Path (Join-Path $PSScriptRoot '..\src\ParallelSpace\bin\Release\net9.0-windows\ParallelSpace.exe')).Path)"
