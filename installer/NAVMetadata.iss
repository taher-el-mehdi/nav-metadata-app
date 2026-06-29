; NAV Metadata — Inno Setup script
; Build: run scripts/build-release.ps1 from the repo root (packages publish output + optional zip + installer)

#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif

#define MyAppName "NAV Metadata"
#define MyAppPublisher "Taher el mehdi"
#define MyAppExeName "NAVMetadata.exe"
#define MyAppURL "https://navmetadata.com"
#define MyAppSupportURL "https://github.com/taher-el-mehdi/nav-metadata/issues"
#define MyAppUpdatesURL "https://github.com/taher-el-mehdi/nav-metadata/releases"
#define PublishDir "..\artifacts\publish\win-x64"
#define MyAppIcon "..\Assets\app-icon.ico"

[Setup]
AppId={{A7B3E901-4C2D-5F8A-9E1B-2D4F6A8C0E35}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppSupportURL}
AppUpdatesURL={#MyAppUpdatesURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=..\artifacts\installer
OutputBaseFilename=NAVMetadata-Setup-{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
SetupIconFile={#MyAppIcon}
UninstallDisplayIcon={app}\{#MyAppExeName}
DisableProgramGroupPage=yes
MinVersion=10.0

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Messages]
WelcomeLabel2=This will install [name/ver] on your computer.%n%nNAV Metadata is a free, open-source toolkit for Microsoft Dynamics NAV metadata. No CAL or finsql required.
