$ErrorActionPreference='Stop'
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes,System.Drawing
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
Add-Type -AssemblyName System.Windows.Forms
Start-Process $installer
$wizard=$null
for($i=0;$i -lt 40;$i++){
 Start-Sleep -Seconds 1
 $wizard=Get-Process|Where-Object {$_.MainWindowTitle -like 'Setup - Sakhr Dictionary Revive*'}|Select-Object -First 1
 if($wizard){break}
}
if(!$wizard){throw 'Installer wizard window not found'}
Start-Sleep -Seconds 2
Capture 'evidence/installer-welcome.png'
$u=[System.Windows.Automation.AutomationElement]
$root=$u::FromHandle($wizard.MainWindowHandle)
function ClickButton($namePart){
 $buttons=$root.FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.PropertyCondition]::new($u::ControlTypeProperty,[System.Windows.Automation.ControlType]::Button))
 foreach($b in $buttons){
  if($b.Current.Name -like "*$namePart*"){
   $b.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
   return $true
  }
 }
 return $false
}
if(!(ClickButton 'Next')){throw 'Next button not found on welcome page'}
Start-Sleep -Seconds 2
if(!(ClickButton 'Next')){throw 'Next button not found on destination page'}
Start-Sleep -Seconds 2
Capture 'evidence/installer-tasks.png'
Get-Process|Where-Object {$_.MainWindowTitle -like 'Setup - Sakhr Dictionary Revive*'}|Stop-Process -Force
Start-Sleep -Seconds 1
Get-Process|Where-Object {$_.Name -like '*SakhrDictionaryRevive*installer*' -or $_.Name -like '*setup-tmp*'}|Stop-Process -Force -ErrorAction SilentlyContinue
'PASS installer wizard captured: welcome and additional tasks pages'|Set-Content evidence/installer-wizard.txt
