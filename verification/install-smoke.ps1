$ErrorActionPreference='Stop'
# Hosted runner is disposable. Only the app's own installed identity is exercised.
$exe=Join-Path $PWD 'setup/SakhrDictionaryRevive-Setup.exe'
foreach($iteration in 1..2){
 $p=Start-Process $exe -ArgumentList '--verify-install' -PassThru
 if(!$p.WaitForExit(180000)){Stop-Process -Id $p.Id -Force;throw 'Install/upgrade timed out'}
 if($p.ExitCode -ne 0){throw "Install/upgrade exit $($p.ExitCode)"}
 $target=Join-Path $env:ProgramFiles 'SakhrDictionaryRevive'
 foreach($f in @('SakhrDictionaryRevive.exe','SakhrDictionaryRevive.dll','Uninstall.exe','data/dictionary.jsonl')){if(!(Test-Path (Join-Path $target $f))){throw "Missing installed file: $f"}}
 $reg=Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SakhrDictionaryRevive'
 if($reg.DisplayName -ne 'Sakhr Dictionary Revive'){throw 'Install identity changed'}
 if($reg.DisplayVersion -ne '1.0.2'){throw 'Wrong version metadata'}
 foreach($shortcut in @((Join-Path ([Environment]::GetFolderPath('CommonPrograms')) 'Sakhr Dictionary Revive.lnk'),(Join-Path ([Environment]::GetFolderPath('CommonDesktopDirectory')) 'Sakhr Dictionary Revive.lnk'))){
  if(!(Test-Path $shortcut)){throw "Missing shortcut: $shortcut"}
  $s=(New-Object -ComObject WScript.Shell).CreateShortcut($shortcut)
  if($s.TargetPath -ne (Join-Path $target 'SakhrDictionaryRevive.exe')){throw 'Wrong shortcut target'}
 }
}
'PASS: clean install and in-place reinstall, file layout, stable identity, registry version and both shortcuts'|Set-Content evidence/install-smoke.txt

# Uninstall through the shipped program, dismissing only its completion dialog.
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes
$u=[System.Windows.Automation.AutomationElement]
$tempUninstaller=Join-Path $env:RUNNER_TEMP 'Sakhr-uninstall-verification.exe'
Copy-Item (Join-Path $target 'Uninstall.exe') $tempUninstaller
$uninstall=Start-Process $tempUninstaller -ArgumentList '--remove' -PassThru
Start-Sleep -Seconds 5
$dialog=$u::RootElement.FindFirst([System.Windows.Automation.TreeScope]::Children,[System.Windows.Automation.PropertyCondition]::new($u::ProcessIdProperty,$uninstall.Id))
if($dialog){
 $ok=$dialog.FindFirst([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.PropertyCondition]::new($u::NameProperty,'OK'))
 if($ok){
  Add-Type -TypeDefinition 'using System; using System.Runtime.InteropServices; public class NativeDialog { [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr h,uint m,IntPtr w,IntPtr l); }'
  [NativeDialog]::SendMessage([IntPtr]$ok.Current.NativeWindowHandle,0x00F5,[IntPtr]::Zero,[IntPtr]::Zero)|Out-Null
 }
}
if(!$uninstall.WaitForExit(30000)){Stop-Process -Id $uninstall.Id -Force;throw 'Uninstall failed to finish'}
if(Test-Path $target){throw 'App install directory remains after uninstall'}
if(Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SakhrDictionaryRevive'){throw 'Uninstall registry key remains'}
'PASS: actual uninstall removes install directory and registry identity'|Add-Content evidence/install-smoke.txt
