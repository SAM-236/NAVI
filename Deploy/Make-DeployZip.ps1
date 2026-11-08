<#
.SYNOPSIS
  Builds NAVI for each Revit year and zips it in the ProgramData\Autodesk\Revit\Addins layout.

.EXAMPLE
  .\Deploy\Make-DeployZip.ps1                    # 2020-2026
  .\Deploy\Make-DeployZip.ps1 -Versions 2020..2027
#>
param(
    [int[]]$Versions = 2020..2026
)

$ErrorActionPreference = 'Stop'
$root    = Split-Path $PSScriptRoot -Parent
$project = Join-Path $root 'NAVI.csproj'
$stage   = Join-Path $root 'obj\deploy-stage'
$zip     = Join-Path $PSScriptRoot ("NAVI_Revit{0}-{1}.zip" -f ($Versions | Measure-Object -Minimum).Minimum, ($Versions | Measure-Object -Maximum).Maximum)

if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory $stage | Out-Null

foreach ($v in $Versions) {
    Write-Host "== Revit $v ==" -ForegroundColor Cyan
    dotnet build $project -c "R$v" -p:SkipRevitDeploy=true --nologo -v minimal
    if ($LASTEXITCODE -ne 0) { throw "Build failed for R$v" }

    $dll = Get-ChildItem (Join-Path $root "bin\R$v") -Recurse -Filter 'NAVI.dll' |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $dll) { throw "NAVI.dll not found for R$v" }

    $target = Join-Path $stage "$v\NAVI"
    New-Item -ItemType Directory -Force $target | Out-Null
    Copy-Item $dll.FullName $target
    Copy-Item (Join-Path $root 'NAVI.addin') (Join-Path $stage $v)
}

New-Item -ItemType Directory (Join-Path $stage 'NAVI') | Out-Null
Copy-Item (Join-Path $root 'CadImportGuard.ini') (Join-Path $stage 'NAVI')
Copy-Item (Join-Path $PSScriptRoot 'README.txt') $stage

if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip
Write-Host "Created $zip" -ForegroundColor Green
