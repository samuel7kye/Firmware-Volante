param(
    [Parameter(Mandatory=$true)][string]$IsccPath,
    [string]$PayloadPath,
    [string]$OutputPath
)
$ErrorActionPreference = 'Stop'
if (!$PayloadPath) { $PayloadPath = Join-Path $PSScriptRoot 'bin' }
if (!$OutputPath) { $OutputPath = Join-Path $PSScriptRoot 'release' }
$IsccPath = (Resolve-Path -LiteralPath $IsccPath).Path
$PayloadPath = (Resolve-Path -LiteralPath $PayloadPath).Path
$required = @('VolanteStudio.exe','VolanteStudio.exe.config','compat\EMC.dll','compat\EMCHid.dll','compat\MaterialDesignColors.dll','compat\MaterialDesignThemes.Wpf.dll')
foreach ($file in $required) {
    if (!(Test-Path -LiteralPath (Join-Path $PayloadPath $file) -PathType Leaf)) { throw "Pacote incompleto: $file. Compile usando build.ps1 com as bibliotecas originais disponíveis." }
}
New-Item -ItemType Directory -Force $OutputPath | Out-Null
$OutputPath = (Resolve-Path -LiteralPath $OutputPath).Path
& $IsccPath "/DPayloadDir=$PayloadPath" "/DOutputPath=$OutputPath" (Join-Path $PSScriptRoot 'installer\VolanteStudio.iss')
if ($LASTEXITCODE -ne 0) { throw 'Falha ao compilar o instalador.' }
$setup = Join-Path $OutputPath 'VolanteStudio-Setup-1.0.3.exe'
$hash = (Get-FileHash -LiteralPath $setup -Algorithm SHA256).Hash.ToLowerInvariant()
Set-Content -LiteralPath ($setup + '.sha256') -Value ($hash + '  ' + [IO.Path]::GetFileName($setup)) -Encoding ascii
Write-Output "Instalador completo: $setup"
