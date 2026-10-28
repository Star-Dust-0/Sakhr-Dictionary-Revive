$ErrorActionPreference='Stop'
# The installer elevates through UAC; driving its window requires the same integrity level.
$principal=[Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
if(-not $principal.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)){
 $repoRoot=Split-Path -Parent (Split-Path -Parent $PSCommandPath)
 Start-Process pwsh -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File',"`"$PSCommandPath`"" -Verb RunAs -WorkingDirectory $repoRoot -Wait
 exit
}
Add-Type -AssemblyName System.Drawing,System.Windows.Forms
Add-Type -TypeDefinition 'using System;using System.Runtime.InteropServices;public class Win32{[DllImport("user32.dll")]public static extern bool SetForegroundWindow(IntPtr hWnd);[DllImport("user32.dll")]public static extern bool ShowWindow(IntPtr hWnd,int nCmdShow);}'
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
$wizard.Refresh()
[Win32]::ShowWindow($wizard.MainWindowHandle,9)|Out-Null
[Win32]::SetForegroundWindow($wizard.MainWindowHandle)|Out-Null
Start-Sleep -Milliseconds 800
# Alt+N activates the Next button mnemonic; after UAC elevation Inno starts on the destination page.
[System.Windows.Forms.SendKeys]::SendWait('%n')
Start-Sleep -Seconds 2
Capture 'evidence/installer-tasks.png'
Get-Process|Where-Object {$_.MainWindowTitle -like 'Setup - Sakhr Dictionary Revive*'}|Stop-Process -Force -ErrorAction SilentlyContinue
'Captured installer destination and additional-tasks pages'|Set-Content evidence/installer-wizard.txt
