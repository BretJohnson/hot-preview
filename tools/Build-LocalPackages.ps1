#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds all NuGet packages in the repository to the bin\Packages\$(Configuration) directory, defaulting to Debug.

.DESCRIPTION
    This script builds all packable projects in the repository. Uses detailed verbosity to
    show all build messages.

.PARAMETER Configuration
    The build configuration to use (Debug or Release). Default is Debug.

.EXAMPLE
    .\Build-LocalPackages.ps1
    Builds all packages in Debug configuration.

.EXAMPLE
    .\Build-LocalPackages.ps1 -Configuration Release
    Builds all packages in Release configuration.
#>

[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'

# Get the directory of this script
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectFile = Join-Path $scriptDir "Build-LocalPackages.proj"

Write-Host "Building local packages..." -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Cyan
Write-Host ""

# Run dotnet build with detailed verbosity
& dotnet build $projectFile -c $Configuration -v detailed

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Build completed successfully!" -ForegroundColor Green
