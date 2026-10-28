[Setup]
AppId=SakhrDictionaryRevive
AppName=Sakhr Dictionary Revive
AppVersion=1.0.3
AppPublisher=Mohamed Elnaggar
AppPublisherURL=https://instinct.com
DefaultDirName={autopf}\SakhrDictionaryRevive
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=out
OutputBaseFilename=SakhrDictionaryRevive-v1.0.3-installer-win-x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupIconFile=publish\app.ico
CloseApplications=yes
RestartApplications=no
UninstallDisplayIcon={app}\SakhrDictionaryRevive.exe

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Sakhr Dictionary Revive"; Filename: "{app}\SakhrDictionaryRevive.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\Sakhr Dictionary Revive"; Filename: "{app}\SakhrDictionaryRevive.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[InstallDelete]
Type: files; Name: "{commondesktop}\Sakhr Dictionary Revive (Self-contained).lnk"
Type: files; Name: "{commonprograms}\Sakhr Dictionary Revive (Self-contained).lnk"
Type: files; Name: "{userdesktop}\Sakhr Dictionary Revive (Self-contained).lnk"
Type: files; Name: "{userprograms}\Sakhr Dictionary Revive (Self-contained).lnk"
Type: files; Name: "{userdesktop}\Sakhr Dictionary Revive.lnk"
Type: files; Name: "{userprograms}\Sakhr Dictionary Revive.lnk"

[Run]
Filename: "{app}\SakhrDictionaryRevive.exe"; Description: "Launch Sakhr Dictionary Revive"; Flags: nowait postinstall skipifsilent

[Messages]
WelcomeLabel2=This package includes .NET and Windows App Runtime. No runtime download is required.%n%nThe app installs into Program Files and replaces any earlier Sakhr Dictionary Revive installation. Your personal settings are kept.%n%nbrought to you by Instinct

[Code]
procedure KillRunningApp;
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{cmd}'), '/c taskkill /F /IM SakhrDictionaryRevive.exe >nul 2>nul', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

procedure RemoveDirIfPresent(const DirPath: string);
begin
  if (DirPath <> '') and DirExists(DirPath) then
    DelTree(DirPath, True, True, True);
end;

procedure UninstallPerUserBuild;
var
  UninstallerPath: string;
  ResultCode: Integer;
begin
  if RegQueryStringValue(HKCU, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SakhrDictionaryRevive-SelfContained_is1', 'UninstallString', UninstallerPath) then
  begin
    UninstallerPath := RemoveQuotes(UninstallerPath);
    if FileExists(UninstallerPath) then
      Exec(UninstallerPath, '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  end;
  RemoveDirIfPresent(ExpandConstant('{localappdata}\Programs\SakhrDictionaryRevive-SelfContained'));
  RegDeleteKeyIncludingSubkeys(HKCU, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SakhrDictionaryRevive-SelfContained_is1');
end;

procedure RemoveLegacyAdminInstall(const KeyName, FallbackDir, ShortcutName: string);
var
  InstallLocation: string;
begin
  if RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\' + KeyName) then
  begin
    InstallLocation := '';
    RegQueryStringValue(HKLM, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\' + KeyName, 'InstallLocation', InstallLocation);
    if InstallLocation = '' then
      InstallLocation := FallbackDir;
    RemoveDirIfPresent(InstallLocation);
    RegDeleteKeyIncludingSubkeys(HKLM, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\' + KeyName);
  end;
  DeleteFile(ExpandConstant('{commonprograms}\' + ShortcutName + '.lnk'));
  DeleteFile(ExpandConstant('{commondesktop}\' + ShortcutName + '.lnk'));
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    KillRunningApp;
    UninstallPerUserBuild;
    RemoveLegacyAdminInstall('SakhrDictionaryRevive', ExpandConstant('{autopf}\SakhrDictionaryRevive'), 'Sakhr Dictionary Revive');
    RemoveLegacyAdminInstall('FluentSakhrDictionary', ExpandConstant('{autopf}\FluentSakhrDictionary'), 'Fluent Sakhr Dictionary');
  end;
end;
