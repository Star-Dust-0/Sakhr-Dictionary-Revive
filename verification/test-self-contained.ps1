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
$programFilesRoot=Join-Path $env:ProgramFiles 'SakhrDictionaryRevive'
$perUserRoot=Join-Path $env:LOCALAPPDATA 'Programs\SakhrDictionaryRevive-SelfContained'
$hkcuKey='HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SakhrDictionaryRevive-SelfContained_is1'
$hklmLegacyKey='HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SakhrDictionaryRevive'
$hklmNewKey='HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SakhrDictionaryRevive_is1'
$hklmAncientKey='HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\FluentSakhrDictionary'
$state=Join-Path $env:APPDATA 'SakhrDictionaryRevive'

function New-Shortcut($path,$target){
 $s=(New-Object -ComObject WScript.Shell).CreateShortcut($path)
 $s.TargetPath=$target
 $s.Save()
}

# --- Stage the machine with every earlier installation shape ---
# 1. The published per-user v1.0.2 build (the real published installer, downloaded anonymously).
$release=Invoke-RestMethod https://api.github.com/repos/StarsDust01/Sakhr-Dictionary-Revive/releases/latest
if($release.tag_name -ne 'v1.0.2'){throw "Unexpected published release for migration baseline: $($release.tag_name)"}
$asset=$release.assets|Where-Object name -eq 'SakhrDictionaryRevive-v1.0.2-installer-win-x64.exe'
if(!$asset){throw 'Published v1.0.2 installer asset missing'}
Invoke-WebRequest $asset.browser_download_url -OutFile old-per-user-installer.exe
$p=Start-Process ./old-per-user-installer.exe -ArgumentList '/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART' -Wait -PassThru
if($p.ExitCode -ne 0){throw "Baseline per-user install failed: $($p.ExitCode)"}
if(!(Test-Path "$perUserRoot/SakhrDictionaryRevive.exe")){throw 'Baseline per-user install missing'}
if(!(Test-Path $hkcuKey)){throw 'Baseline per-user registry entry missing'}
# Personal settings that must survive migration.
New-Item -ItemType Directory -Force $state|Out-Null
'{"Language":"ar","Theme":"Dark","Marker":"migration-keep"}'|Set-Content (Join-Path $state 'settings.json')
# 2. The original framework-dependent admin install (custom installer, v1.0.0-1.0.1 era).
New-Item -ItemType Directory -Force $programFilesRoot|Out-Null
'stale framework-dependent file'|Set-Content (Join-Path $programFilesRoot 'old-framework-file.txt')
New-Item -Path $hklmLegacyKey -Force|Out-Null
Set-ItemProperty $hklmLegacyKey -Name DisplayName -Value 'Sakhr Dictionary Revive'
Set-ItemProperty $hklmLegacyKey -Name InstallLocation -Value $programFilesRoot
Set-ItemProperty $hklmLegacyKey -Name UninstallString -Value "`"$programFilesRoot\Uninstall.exe`" --uninstall"
New-Shortcut (Join-Path ([Environment]::GetFolderPath('CommonDesktopDirectory')) 'Sakhr Dictionary Revive.lnk') "$programFilesRoot\Uninstall.exe"
New-Shortcut (Join-Path ([Environment]::GetFolderPath('CommonPrograms')) 'Sakhr Dictionary Revive.lnk') "$programFilesRoot\Uninstall.exe"
# 3. The v0.x-era admin install under the pre-rename identity.
$ancientRoot=Join-Path $env:ProgramFiles 'FluentSakhrDictionary'
New-Item -ItemType Directory -Force $ancientRoot|Out-Null
'stale v0.x file'|Set-Content (Join-Path $ancientRoot 'old-v0-file.txt')
New-Item -Path $hklmAncientKey -Force|Out-Null
Set-ItemProperty $hklmAncientKey -Name DisplayName -Value 'Fluent Sakhr Dictionary'
Set-ItemProperty $hklmAncientKey -Name InstallLocation -Value $ancientRoot
New-Shortcut (Join-Path ([Environment]::GetFolderPath('CommonDesktopDirectory')) 'Fluent Sakhr Dictionary.lnk') "$ancientRoot\Uninstall.exe"

# --- Migrate: install the new admin build over all of that ---
$p=Start-Process $installer -ArgumentList '/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART' -PassThru -Wait
if($p.ExitCode -ne 0){throw "Admin install exit $($p.ExitCode)"}

# New install landed in Program Files with clean identity.
if(!(Test-Path "$programFilesRoot/SakhrDictionaryRevive.exe")){throw 'Program Files install missing'}
if(Test-Path "$programFilesRoot/old-framework-file.txt"){throw 'Stale framework-dependent file survived'}
$reg=Get-ItemProperty $hklmNewKey
$reg|Select-Object DisplayName,DisplayVersion,InstallLocation,UninstallString|ConvertTo-Json|Set-Content evidence/install-registry.json
if($reg.DisplayName -notlike 'Sakhr Dictionary Revive*'){throw "Unexpected DisplayName: $($reg.DisplayName)"}
if("$($reg.DisplayName)" -match 'Self'){throw "Self-contained wording in DisplayName: $($reg.DisplayName)"}
if($reg.DisplayVersion -ne '1.0.3'){throw "Version mismatch: $($reg.DisplayVersion)"}
# Every earlier shape is gone.
if(Test-Path "$perUserRoot/SakhrDictionaryRevive.exe"){throw 'Per-user install survived migration'}
if(Test-Path $perUserRoot){throw 'Per-user folder survived migration'}
if(Test-Path $hkcuKey){throw 'Per-user registry entry survived migration'}
if(Test-Path $hklmLegacyKey){throw 'Legacy admin registry entry survived migration'}
if(Test-Path $hklmAncientKey){throw 'Ancient v0.x registry entry survived migration'}
if(Test-Path $ancientRoot){throw 'Ancient v0.x folder survived migration'}
# Settings preserved.
$saved=Get-Content (Join-Path $state 'settings.json') -Raw|ConvertFrom-Json
if($saved.Marker -ne 'migration-keep'){throw 'Personal settings lost during migration'}
# Shortcuts: exactly one clean-name pair pointing at Program Files, nothing else.
foreach($folder in @([Environment]::GetFolderPath('CommonDesktopDirectory'),[Environment]::GetFolderPath('CommonPrograms'),[Environment]::GetFolderPath('Desktop'),[Environment]::GetFolderPath('Programs'))){
 if(Test-Path (Join-Path $folder 'Sakhr Dictionary Revive (Self-contained).lnk')){throw "Self-contained shortcut remains in $folder"}
 if(Test-Path (Join-Path $folder 'Fluent Sakhr Dictionary.lnk')){throw "Ancient shortcut remains in $folder"}
}
foreach($folder in @([Environment]::GetFolderPath('CommonDesktopDirectory'),[Environment]::GetFolderPath('CommonPrograms'))){
 $link=Join-Path $folder 'Sakhr Dictionary Revive.lnk'
 if(!(Test-Path $link)){throw "Clean-name shortcut missing in $folder"}
 $shortcut=(New-Object -ComObject WScript.Shell).CreateShortcut($link)
 if($shortcut.TargetPath -ne "$programFilesRoot\SakhrDictionaryRevive.exe"){throw "Wrong shortcut target: $($shortcut.TargetPath)"}
}
# Reinstall over itself must stay clean (single registry entry, no duplicate).
$p=Start-Process $installer -ArgumentList '/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART' -PassThru -Wait
if($p.ExitCode -ne 0){throw "Reinstall exit $($p.ExitCode)"}
$entries=@(Get-ChildItem 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall'|Where-Object {$_.PSChildName -like 'SakhrDictionaryRevive*' -or $_.PSChildName -like 'FluentSakhr*'})
if($entries.Count -ne 1 -or $entries[0].PSChildName -ne 'SakhrDictionaryRevive_is1'){throw "Unexpected uninstall entries: $($entries.PSChildName -join ',')"}
# The desktop shortcut option is honored both ways.
$commonDesktop=[Environment]::GetFolderPath('CommonDesktopDirectory')
$p=Start-Process $installer -ArgumentList '/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART','/TASKS=!desktopicon' -PassThru -Wait
if($p.ExitCode -ne 0){throw "Task-disabled install exit $($p.ExitCode)"}
if(Test-Path (Join-Path $commonDesktop 'Sakhr Dictionary Revive.lnk')){throw 'Desktop icon created despite the task being disabled'}
# Inno remembers the last task selection on reinstall, so re-enable explicitly here;
# the default-checked state was already proven by the first fresh install above.
$p=Start-Process $installer -ArgumentList '/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART','/TASKS=desktopicon' -PassThru -Wait
if($p.ExitCode -ne 0){throw "Reinstall exit $($p.ExitCode)"}
if(!(Test-Path (Join-Path $commonDesktop 'Sakhr Dictionary Revive.lnk'))){throw 'Desktop icon missing after explicit-task reinstall'}
# Installed app launches and renders.
$p=Start-Process "$programFilesRoot/SakhrDictionaryRevive.exe" -ArgumentList '--screenshot',"$PWD/evidence/installed.png",'--shot-word','CAT' -PassThru
if(!$p.WaitForExit(90000)){Stop-Process -Id $p.Id -Force;throw 'Installed capture timed out'}
if(!(Test-Path evidence/installed.png)){throw 'Installed capture missing'}
# Uninstall works from Program Files; personal settings stay.
$p=Start-Process "$programFilesRoot/unins000.exe" -ArgumentList '/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART' -Wait -PassThru
if($p.ExitCode -ne 0 -or(Test-Path "$programFilesRoot/SakhrDictionaryRevive.exe")){throw 'Admin uninstall failed'}
if(Test-Path $hklmNewKey){throw 'Admin uninstall registry remains'}
if(!(Test-Path (Join-Path $state 'settings.json'))){throw 'Settings removed by uninstall'}
Remove-Item (Join-Path $state 'settings.json') -Force
'PASS migration from per-user and both legacy admin installs, Program Files install/reinstall, clean identity and shortcuts, desktop-option both states, settings preserved, installed capture and uninstall'|Set-Content evidence/self-contained-tests.txt
