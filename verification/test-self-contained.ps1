$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force evidence|Out-Null
Get-AppxPackage '*WindowsAppRuntime*'|Remove-AppxPackage -ErrorAction Stop
if(Get-AppxPackage '*WindowsAppRuntime*'){throw 'Runner shared Windows App Runtime remains'}
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
$root="$env:LOCALAPPDATA/Programs/SakhrDictionaryRevive-SelfContained"
if(!(Test-Path "$root/SakhrDictionaryRevive.exe")){throw 'Per-user install missing'}
$p=Start-Process "$root/SakhrDictionaryRevive.exe" -ArgumentList '--screenshot',"$PWD/evidence/installed.png",'--shot-word','CAT' -PassThru
if(!$p.WaitForExit(90000)){Stop-Process -Id $p.Id -Force;throw 'Installed capture timed out'}
if(!(Test-Path evidence/installed.png)){throw 'Installed capture missing'}
$p=Start-Process "$root/unins000.exe" -ArgumentList '/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART' -Wait -PassThru
if($p.ExitCode -ne 0 -or(Test-Path "$root/SakhrDictionaryRevive.exe")){throw 'Per-user uninstall failed'}
'PASS isolated app-local runtime portable launch, per-user install/reinstall, installed capture and uninstall'|Set-Content evidence/self-contained-tests.txt
