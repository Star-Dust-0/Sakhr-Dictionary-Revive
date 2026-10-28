$ErrorActionPreference='Stop'
# The installer elevates through UAC; driving its window requires the same integrity level.
$principal=[Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
if(-not $principal.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)){
 $repoRoot=Split-Path -Parent (Split-Path -Parent $PSCommandPath)
 Start-Process pwsh -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File',"`"$PSCommandPath`"" -Verb RunAs -WorkingDirectory $repoRoot -Wait
 exit
}
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes,System.Drawing,System.Windows.Forms
New-Item -ItemType Directory -Force evidence|Out-Null
$installer=(Get-ChildItem out/*installer*.exe|Select-Object -First 1).FullName
function Capture($file){
 $bounds=[System.Windows.Forms.Screen]::PrimaryScreen.Bounds
 $bitmap=New-Object System.Drawing.Bitmap $bounds.Width,$bounds.Height
 $graphics=[System.Drawing.Graphics]::FromImage($bitmap)
 $graphics.CopyFromScreen($bounds.Location,[System.Drawing.Point]::Empty,$bounds.Size)
 $bitmap.Save("$PWD/$file")
 $graphics.Dispose();$bitmap.Dispose()
}
Start-Process $installer
$wizard=$null
for($i=0;$i -lt 40;$i++){
 Start-Sleep -Seconds 1
 $wizard=Get-Process|Where-Object {$_.MainWindowTitle -like 'Setup - Sakhr Dictionary Revive*'}|Select-Object -First 1
 if($wizard){break}
}
if(!$wizard){throw 'Installer wizard window not found'}
Start-Sleep -Seconds 2
Capture 'evidence/installer-destination.png'
$u=[System.Windows.Automation.AutomationElement]
$root=$u::FromHandle($wizard.MainWindowHandle)
function ClickButton($namePart){
 $wizard.Refresh()
 $r=$u::FromHandle($wizard.MainWindowHandle)
 $buttons=$r.FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.PropertyCondition]::new($u::ControlTypeProperty,[System.Windows.Automation.ControlType]::Button))
 $names=@()
 foreach($b in $buttons){
  $names+=$b.Current.Name
  if($b.Current.Name -like "*$namePart*"){
   $b.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
   return ''
  }
 }
 return "buttons seen: $($names -join ' | ')"
}
$err=ClickButton 'Next'
if($err){throw "Next button not found on destination page - $err"}
Start-Sleep -Seconds 2
Capture 'evidence/installer-tasks.png'
# Verify the desktop shortcut option is actually on this page.
$wizard.Refresh()
$r=$u::FromHandle($wizard.MainWindowHandle)
$text=$r.FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.PropertyCondition]::new($u::ControlTypeProperty,[System.Windows.Automation.ControlType]::CheckBox))
$boxNames=@($text|ForEach-Object {$_.Current.Name})
if(!($boxNames|Where-Object {$_ -like '*desktop*'})){throw "Desktop shortcut option not found; checkboxes: $($boxNames -join ' | ')"}
"Desktop option present: $($boxNames -join ' | ')"|Set-Content evidence/installer-wizard.txt
Get-Process|Where-Object {$_.MainWindowTitle -like 'Setup - Sakhr Dictionary Revive*'}|Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1
Get-ChildItem "$env:TEMP" -Filter 'is-*.tmp' -Directory -ErrorAction SilentlyContinue|ForEach-Object {Get-Process -Id (Get-ChildItem $_.FullName -Filter *.exe -ErrorAction SilentlyContinue|ForEach-Object {$_.Name -replace '\.exe$',''}|Where-Object {$_ -match '^\d+$'}) -ErrorAction SilentlyContinue}
