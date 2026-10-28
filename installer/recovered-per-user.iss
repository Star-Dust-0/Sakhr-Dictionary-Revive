[Setup]
AppId=SakhrDictionaryRevive-SelfContained
AppName=Sakhr Dictionary Revive
AppVersion=1.0.2
AppPublisher=Mohamed Elnaggar
AppPublisherURL=https://instinct.com
DefaultDirName={localappdata}\Programs\SakhrDictionaryRevive-SelfContained
DefaultGroupName=Sakhr Dictionary Revive
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=out
OutputBaseFilename=SakhrDictionaryRevive-v1.0.2-installer-win-x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupIconFile=publish\app.ico
CloseApplications=yes
RestartApplications=no
UninstallDisplayIcon={app}\SakhrDictionaryRevive.exe
[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{autoprograms}\Sakhr Dictionary Revive"; Filename: "{app}\SakhrDictionaryRevive.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\Sakhr Dictionary Revive"; Filename: "{app}\SakhrDictionaryRevive.exe"; WorkingDir: "{app}"
[InstallDelete]
Type: files; Name: "{autodesktop}\Sakhr Dictionary Revive (Self-contained).lnk"
Type: files; Name: "{autoprograms}\Sakhr Dictionary Revive (Self-contained).lnk"
[Run]
Filename: "{app}\SakhrDictionaryRevive.exe"; Description: "Launch Sakhr Dictionary Revive"; Flags: nowait postinstall skipifsilent
[Messages]
WelcomeLabel2=This edition includes .NET and Windows App Runtime. No runtime download or administrator rights are required.%n%nbrought to you by Instinct
