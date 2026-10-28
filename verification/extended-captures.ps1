$ErrorActionPreference = 'Stop'
$state = Join-Path $env:APPDATA 'SakhrDictionaryRevive'
foreach ($flavor in @('original-run','publish')) {
 foreach ($lang in @('en','ar')) {
  foreach ($size in @('800x600','1600x900')) {
   foreach ($backdrop in @('Mica','MicaAlt','Acrylic')) {
    @{Language=$lang;Theme='Dark';Backdrop=$backdrop;Accent='#C239B3';UseWiktionary=$true} | ConvertTo-Json | Set-Content (Join-Path $state 'settings.json')
    $file = Join-Path $PWD "evidence/extended-$flavor-$lang-$size-$backdrop.png"
    $arg = @('--screenshot',$file,'--shot-word','كتاب',"--shot-size=$size")
    $p = Start-Process (Join-Path $PWD "$flavor/SakhrDictionaryRevive.exe") -ArgumentList $arg -PassThru
    if (!$p.WaitForExit(90000)) {Stop-Process -Id $p.Id -Force;throw "Extended capture hung: $file"}
    if ($p.ExitCode -ne 0 -or !(Test-Path $file)) {throw "Extended capture failed: $file"}
   }
  }
 }
}
foreach ($mode in @('--preview','--preview-dark','--preview-busy','--preview-done')) {
 $file=Join-Path $PWD "evidence/installer-$($mode.TrimStart('-')).png"
 $p=Start-Process setup/SakhrDictionaryRevive-Setup.exe -ArgumentList @($mode,$file) -PassThru
 if (!$p.WaitForExit(60000)) {Stop-Process -Id $p.Id -Force;throw "Installer preview hung: $mode"}
 if (!(Test-Path $file)) {throw "Installer preview missing: $mode"}
}
