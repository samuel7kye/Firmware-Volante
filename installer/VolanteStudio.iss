#ifndef PayloadDir
  #define PayloadDir "..\bin"
#endif
#ifndef OutputPath
  #define OutputPath "..\release"
#endif
[Setup]
AppId={{A75EFC4E-2AE0-4F4E-9624-51637C18860B}
AppName=Volante Studio
AppVersion=1.0.0
AppPublisher=Projeto Volante Studio
AppPublisherURL=https://github.com/samuel7kye/Firmware-Volante
AppSupportURL=https://github.com/samuel7kye/Firmware-Volante/issues
DefaultDirName={localappdata}\Programs\Volante Studio
DefaultGroupName=Volante Studio
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
MinVersion=10.0
OutputDir={#OutputPath}
OutputBaseFilename=VolanteStudio-Setup-1.0.0
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupLogging=yes
UninstallDisplayIcon={app}\VolanteStudio.exe
AppMutex=Local\VolanteStudio.Desktop
CloseApplications=no
RestartApplications=no
InfoBeforeFile=ANTES-DE-INSTALAR.txt
VersionInfoDescription=Instalador do Volante Studio
VersionInfoProductName=Volante Studio
VersionInfoVersion=1.0.0.0

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar um atalho na área de trabalho"; GroupDescription: "Atalhos:"

[Files]
Source: "{#PayloadDir}\VolanteStudio.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PayloadDir}\VolanteStudio.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PayloadDir}\compat\EMC.dll"; DestDir: "{app}\compat"; Flags: ignoreversion
Source: "{#PayloadDir}\compat\EMCHid.dll"; DestDir: "{app}\compat"; Flags: ignoreversion
Source: "{#PayloadDir}\compat\MaterialDesignColors.dll"; DestDir: "{app}\compat"; Flags: ignoreversion
Source: "{#PayloadDir}\compat\MaterialDesignThemes.Wpf.dll"; DestDir: "{app}\compat"; Flags: ignoreversion
Source: "PRIMEIROS-PASSOS.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\THIRD_PARTY.md"; DestDir: "{app}"; DestName: "COMPONENTES-DE-TERCEIROS.txt"; Flags: ignoreversion

[Icons]
Name: "{group}\Volante Studio"; Filename: "{app}\VolanteStudio.exe"; WorkingDir: "{app}"
Name: "{group}\Primeiros passos"; Filename: "{app}\PRIMEIROS-PASSOS.txt"
Name: "{autodesktop}\Volante Studio"; Filename: "{app}\VolanteStudio.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\VolanteStudio.exe"; Description: "Abrir o Volante Studio"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
var
  Release: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM32, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release);
  if Result then Result := Release >= 394254;
  if not Result then
    MsgBox('O Volante Studio precisa do .NET Framework 4.6.1 ou superior. Atualize o Windows ou instale o .NET Framework 4.8 pelo site da Microsoft e tente novamente.', mbError, MB_OK);
end;

// No UninstallDelete rule: user-created Data/Profiles files survive updates and uninstall.
