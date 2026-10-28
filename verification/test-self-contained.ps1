$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force evidence|Out-Null
# Appx module requires Windows PowerShell, not PowerShell 7 on this runner.
& powershell.exe -NoProfile -Command '$ErrorActionPreference="Stop"; Get-AppxPackage "*WindowsAppRuntime*"|Remove-AppxPackage -ErrorAction Stop; if(Get-AppxPackage "*WindowsAppRuntime*"){throw "Shared Windows App Runtime remains"}'
if($LASTEXITCODE -ne 0){throw 'Shared runtime removal check failed'}
$env:DOTNET_MULTILEVEL_LOOKUP='0'
$env:DOTNET_ROOT="$env:RUNNER_TEMP/no-shared-dotnet"
New-Item -ItemType Directory -Force $env:DOTNET_ROOT|Out-Null
$p=Start-Process "$PWD/publish/SakhrDictionaryRevive.exe" -PassThru
Start-Sleep -Seconds 15
if($p.HasExited){throw "Portable exit $($p.ExitCode)"}
$local=(Resolve-Path publish).Path
$modules=@($p.Modules|Where-Object {$_.ModuleName -match '^(coreclr|Microsoft.WinUI|Microsoft.WindowsAppRuntime|hostfxr)\.dll$'})
foreach($name in @('coreclr.dll','Microsoft.WinUI.dll','Microsoft.WindowsAppRuntime.dll')){
 $m=$modules|Where-Object ModuleName -eq $name
 if(!$m -or !$m.FileName.StartsWith($local,[StringComparison]::OrdinalIgnoreCase)){throw "Not app-local: $name"}
}
$modules|Select-Object ModuleName,FileName|ConvertTo-Json|Set-Content evidence/dependency-proof.json
Stop-Process -Id $p.Id -Force
$installer=(Get-ChildItem out/*installer*.exe|Select-Object -First 1).FullName
foreach($i in 1..2){
 $p=Start-Process $installer -ArgumentList '/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART' -PassThru -Wait
 if($p.ExitCode -ne 0){throw "Per-user install exit $($p.ExitCode)"}
}
$root=Join-Path $env:LOCALAPPDATA 'Programs\SakhrDictionaryRevive-SelfContained'
if(!(Test-Path "$root/SakhrDictionaryRevive.exe")){throw 'Per-user install missing'}
$registry='HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SakhrDictionaryRevive-SelfContained_is1'
$reg=Get-ItemProperty $registry
$reg|Select-Object DisplayName,DisplayVersion,InstallLocation,UninstallString|ConvertTo-Json|Set-Content evidence/install-registry.json
if($reg.DisplayName -notin @('Sakhr Dictionary Revive','Sakhr Dictionary Revive version 1.0.2') -or $reg.DisplayVersion -ne '1.0.2'){throw "Per-user registry identity/version mismatch: $($reg.DisplayName) / $($reg.DisplayVersion)"}
foreach($folder in @([Environment]::GetFolderPath('Desktop'),[Environment]::GetFolderPath('Programs'))){
 $link=Join-Path $folder 'Sakhr Dictionary Revive.lnk'
 if(!(Test-Path $link)){throw 'Clean-name shortcut missing'}
 $shortcut=(New-Object -ComObject WScript.Shell).CreateShortcut($link)
 if($shortcut.TargetPath -ne "$root\SakhrDictionaryRevive.exe"){throw 'Wrong per-user shortcut target'}
 if(Test-Path (Join-Path $folder 'Sakhr Dictionary Revive (Self-contained).lnk')){throw 'Stale shortcut remains'}
}
$p=Start-Process "$root/SakhrDictionaryRevive.exe" -ArgumentList '--screenshot',"$PWD/evidence/installed.png",'--shot-word','CAT' -PassThru
if(!$p.WaitForExit(90000)){Stop-Process -Id $p.Id -Force;throw 'Installed capture timed out'}
if(!(Test-Path evidence/installed.png)){throw 'Installed capture missing'}
$p=Start-Process "$root/unins000.exe" -ArgumentList '/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART' -Wait -PassThru
if($p.ExitCode -ne 0 -or(Test-Path "$root/SakhrDictionaryRevive.exe")){throw 'Per-user uninstall failed'}
if(Test-Path $registry){throw 'Per-user uninstall registry remains'}
'PASS isolated app-local runtime portable launch, per-user install/reinstall, installed capture and uninstall'|Set-Content evidence/self-contained-tests.txt
