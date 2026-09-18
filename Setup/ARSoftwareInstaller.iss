#define MyAppName "AR Trading - Jobwork Premium"
#define MyAppVersion "2.4.8"
#define MyAppPublisher "AR Software"
#define MyAppExeName "ARSoftware.exe"

[Setup]
AppId={{AR-TRADING-JOBWORK-PREMIUM-2024}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://arsoftware.com
DefaultDirName={autopf}\AR Trading
DefaultGroupName=AR Trading
AllowNoIcons=yes
OutputDir=D:\ARSoftware\Setup
OutputBaseFilename=AR_Trading_Setup_v{#MyAppVersion}_LIVE
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
DisableDirPage=no
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "D:\ARSoftware\PublishSelfContained\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent