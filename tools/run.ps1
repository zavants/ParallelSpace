# Builds (if needed) and runs ParallelSpace.
$ErrorActionPreference = 'Stop'
$dotnet = 'C:\Program Files\dotnet\dotnet.exe'
if (-not (Test-Path $dotnet)) { $dotnet = 'dotnet' }

$proj = Join-Path $PSScriptRoot '..\src\ParallelSpace\ParallelSpace.csproj'
& $dotnet run --project $proj -c Release
