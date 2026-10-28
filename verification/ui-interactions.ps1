$ErrorActionPreference='Stop'
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes
$u=[System.Windows.Automation.AutomationElement]
$state=Join-Path $env:APPDATA 'SakhrDictionaryRevive'
New-Item -ItemType Directory -Force $state,evidence | Out-Null
@{Language='en';Theme='Light';Backdrop='Mica';Accent='';UseWiktionary=$false}|ConvertTo-Json|Set-Content (Join-Path $state settings.json)
$env:SAKHR_VERIFY='1'
$p=Start-Process "$PWD/publish/SakhrDictionaryRevive.exe" -PassThru
try {
 for($wait=0;$wait -lt 45;$wait++){ $p.Refresh();if($p.HasExited -or $p.MainWindowHandle -ne [IntPtr]::Zero){break};Start-Sleep -Seconds 1 }
 $p.Refresh()
 if($p.HasExited){throw 'App exited before interaction checks'}
 $root=$u::FromHandle($p.MainWindowHandle)
 function FindId($id) {
  return $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.PropertyCondition]::new($u::AutomationIdProperty,$id))
 }
 $search=FindId 'SearchBox'
 if(!$search){throw 'SearchBox is not exposed to UI automation'}
 $pattern=$search.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
 $pattern.SetValue('CAT')
 Start-Sleep -Seconds 2
 $title=FindId 'WordTitle'
 # Typing populates suggestions; selection is tested through ListView's selection pattern.
 $list=FindId 'ResultsList'
 $item=$list.FindFirst([System.Windows.Automation.TreeScope]::Children,[System.Windows.Automation.PropertyCondition]::new($u::ControlTypeProperty,[System.Windows.Automation.ControlType]::ListItem))
 if(!$item){throw 'No suggestions after CAT input'}
 $item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
 Start-Sleep -Milliseconds 500
 if($title.Current.Name -ne 'Cat'){throw "Selection did not update meaning title: $($title.Current.Name)"}
 $second=Start-Process "$PWD/publish/SakhrDictionaryRevive.exe" -PassThru
 if(!$second.WaitForExit(10000)){Stop-Process -Id $second.Id -Force;throw 'Single-instance check failed'}
 foreach($nav in @('NavSettings','NavAbout','NavSearch')) {
  $control=FindId $nav
  if(!$control){throw "Missing navigation automation: $nav"}
  $control.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
  Start-Sleep -Milliseconds 800
 }
 # Exercise actual settings controls, not only JSON round trips.
 (FindId 'NavSettings').GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
 Start-Sleep -Seconds 1
 foreach($id in @('ThemeDark','ThemeLight','BackdropMicaAlt','BackdropAcrylic','BackdropMica')) {
  (FindId $id).GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
  Start-Sleep -Milliseconds 500
 }
 $toggle=FindId 'WiktionaryToggle'
 $toggle.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Toggle()
 Start-Sleep -Milliseconds 500
 if(!(Get-Content (Join-Path $state 'settings.json')|ConvertFrom-Json).UseWiktionary){throw 'Wiktionary toggle not persisted'}
 $toggle.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Toggle()
 (FindId 'AccentCustomButton').GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
 Start-Sleep -Seconds 1
 $done=$root.FindFirst([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.PropertyCondition]::new($u::NameProperty,'Done'))
 if(!$done){throw 'Custom color dialog did not open'}
 $done.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
 Start-Sleep -Seconds 2
 $root=$u::FromHandle($p.MainWindowHandle)
 (FindId 'AccentResetButton').GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
 Start-Sleep -Seconds 1
 # Change language through the actual radio group and verify page rebuild + saved value.
 $picker=FindId 'LanguagePicker'
 $ar=$picker.FindFirst([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.PropertyCondition]::new($u::NameProperty,'العربية'))
 if(!$ar){throw 'Arabic language radio missing'}
 Add-Type -TypeDefinition 'using System; using System.Runtime.InteropServices; public class NativeMouse { [DllImport("user32.dll")] public static extern bool SetCursorPos(int x,int y); [DllImport("user32.dll")] public static extern void mouse_event(uint f,uint x,uint y,uint d,UIntPtr e); }'
 function ClickControl($control){$b=$control.Current.BoundingRectangle;[NativeMouse]::SetCursorPos([int]($b.X+$b.Width/2),[int]($b.Y+$b.Height/2))|Out-Null;[NativeMouse]::mouse_event(2,0,0,0,[UIntPtr]::Zero);[NativeMouse]::mouse_event(4,0,0,0,[UIntPtr]::Zero)}
 ClickControl $ar
 Start-Sleep -Seconds 2
 if((Get-Content (Join-Path $state 'settings.json')|ConvertFrom-Json).Language -ne 'ar'){
 Get-Content (Join-Path $state 'settings.json') | Set-Content evidence/language-failure-settings.json
 Copy-Item (Join-Path $state 'language-debug.log') evidence/language-debug.log -ErrorAction SilentlyContinue
 $root.FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object { "$($_.Current.AutomationId) | $($_.Current.Name) | $($_.Current.ControlType.ProgrammaticName)" } | Set-Content evidence/language-failure-uia.txt
 throw 'Arabic language change did not persist'
 }
 $picker=FindId 'LanguagePicker'
 $en=$picker.FindFirst([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.PropertyCondition]::new($u::NameProperty,'English'))
 if(!$en){throw 'English language radio missing after page rebuild'}
 ClickControl $en
 Start-Sleep -Seconds 2
 if((Get-Content (Join-Path $state 'settings.json')|ConvertFrom-Json).Language -ne 'en'){throw 'English language change did not persist'}
 (FindId 'NavAbout').GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
 Start-Sleep -Seconds 1
 (FindId 'CheckButton').GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
 Start-Sleep -Seconds 23
 $msg=(FindId 'UpdateResult').Current.Name
 if([string]::IsNullOrWhiteSpace($msg) -or $msg -eq 'Checking...'){throw 'Update check never completed'}
 if(!(FindId 'CheckButton').Current.IsEnabled){throw 'Update button remained disabled'}
 "PASS: input, suggestion selection, navigation, single instance, theme/backdrop controls, Wiktionary persistence, custom accent dialog/reset, interactive Arabic/English language switching and update-check completion. Update result: $msg" | Set-Content evidence/ui-interactions.txt
} finally { if(!$p.HasExited){Stop-Process -Id $p.Id -Force} }
