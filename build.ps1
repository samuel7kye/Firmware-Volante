param([string]$OriginalPath = 'C:\Program Files (x86)\EMCLite', [switch]$Test)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
if (!(Test-Path (Join-Path $framework 'csc.exe'))) { $framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319' }
$wpf = Join-Path $framework 'WPF'
$bin = Join-Path $root 'bin'
New-Item -ItemType Directory -Force $bin, (Join-Path $bin 'compat') | Out-Null
$refs = @('System.dll','System.Core.dll','System.Xml.dll','System.Xml.Linq.dll','System.Xaml.dll','System.Management.dll',"$wpf\PresentationFramework.dll","$wpf\PresentationCore.dll","$wpf\WindowsBase.dll") | ForEach-Object { '/r:' + $_ }
# EMCHid uses 32-bit native USB structures; AnyCPU silently fails on 64-bit Windows.
$sources = Get-ChildItem -LiteralPath (Join-Path $root 'src') -Filter '*.cs' | ForEach-Object FullName
& (Join-Path $framework 'csc.exe') /nologo /target:winexe "/win32icon:$root\src\VolanteStudio.ico" /optimize+ /platform:x86 /utf8output "/out:$bin\VolanteStudio.exe" "/win32manifest:$root\src\app.manifest" "/resource:$root\src\MainWindow.xaml,MainWindow.xaml" "/resource:$root\docs\icone.png,StudioLogo.png" @refs @sources
if ($LASTEXITCODE -ne 0) { throw 'Falha ao compilar o aplicativo.' }
Copy-Item -LiteralPath (Join-Path $root 'src\App.config') -Destination (Join-Path $bin 'VolanteStudio.exe.config')
foreach ($dll in @('EMC.dll','EMCHid.dll','MaterialDesignColors.dll','MaterialDesignThemes.Wpf.dll')) {
    $source = Join-Path $OriginalPath $dll
    if (Test-Path $source) { Copy-Item -LiteralPath $source -Destination (Join-Path $bin 'compat') }
    else { Write-Warning "$dll não encontrada. Para incluir a comunicação, informe -OriginalPath com a pasta compat do pacote completo." }
}
if ($Test) {
    & (Join-Path $framework 'csc.exe') /nologo /target:exe /platform:x86 "/out:$bin\Tests.exe" /main:VolanteStudio.Tests "/resource:$root\src\MainWindow.xaml,MainWindow.xaml" "/resource:$root\docs\icone.png,StudioLogo.png" @refs @sources (Join-Path $root 'tests\Tests.cs')
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao compilar os testes.' }
    & (Join-Path $bin 'Tests.exe')
    if ($LASTEXITCODE -ne 0) { throw 'Falha nos testes.' }
}
Write-Output "Aplicativo pronto: $bin\VolanteStudio.exe"
