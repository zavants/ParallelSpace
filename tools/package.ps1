# Publishes a self-contained single-file Windows build and zips a release.
$ErrorActionPreference = 'Stop'
$dotnet = 'C:\Program Files\dotnet\dotnet.exe'
if (-not (Test-Path $dotnet)) { $dotnet = 'dotnet' }

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$proj = Join-Path $root 'src\ParallelSpace\ParallelSpace.csproj'

# Single source of truth for the version: the csproj.
$version = ([xml](Get-Content $proj)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
if (-not $version) { throw "No <Version> found in $proj" }

$rid  = 'win-x64'
$name = "ParallelSpace-v$version-$rid"

Write-Host "Publishing $name ..."
& $dotnet publish $proj -c Release -r $rid --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishTrimmed=false -p:DebugType=none --nologo

$pubExe = Join-Path $root "src\ParallelSpace\bin\Release\net9.0-windows\$rid\publish\ParallelSpace.exe"
$dist   = Join-Path $root 'dist'
$stage  = Join-Path $dist $name
New-Item -ItemType Directory -Force -Path $stage | Out-Null

Copy-Item $pubExe                     (Join-Path $stage 'ParallelSpace.exe') -Force
Copy-Item (Join-Path $root 'README.md') $stage -Force
Copy-Item (Join-Path $root 'LICENSE')   $stage -Force

$zip = Join-Path $dist "$name.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -Force

$size = [math]::Round((Get-Item $zip).Length / 1MB, 1)
Write-Host "`nRelease ready: $zip  ($size MB)"
