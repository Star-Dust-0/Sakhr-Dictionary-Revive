$ErrorActionPreference = 'Stop'
New-Item -ItemType Directory -Force evidence, original-run | Out-Null
# Reuse compatible third-party runtime files from the reconstructed publish.
# App executable, assembly, deps/runtimeconfig and PRI all come from the reference.
Copy-Item publish/* original-run/ -Recurse -Force
Copy-Item verification/original-v1.0.2/* original-run/ -Force
$state = Join-Path $env:APPDATA 'SakhrDictionaryRevive'
New-Item -ItemType Directory -Force $state | Out-Null
foreach ($flavor in @('original-run','publish')) {
  foreach ($lang in @('en','ar')) {
    foreach ($theme in @('Light','Dark')) {
      foreach ($case in @('home','cat','arabic','inflection','missing','settings','about','about-end')) {
        @{Language=$lang;Theme=$theme;Backdrop='Mica';Accent='';UseWiktionary=$false} | ConvertTo-Json | Set-Content (Join-Path $state 'settings.json')
        $file = Join-Path $PWD "evidence/$flavor-$lang-$theme-$case.png"
        $arg = @('--screenshot', $file, '--theme', $theme)
        if ($case -eq 'cat') {$arg += @('--shot-word','CAT')}
        if ($case -eq 'arabic') {$arg += @('--shot-word','قط')}
        if ($case -eq 'inflection') {$arg += @('--shot-word','ABANDONS')}
        if ($case -eq 'missing') {$arg += @('--shot-word','ZZZZZZ-NONEXISTENT')}
        if ($case -in @('settings','about','about-end')) {$arg += @('--shot-page',$case)}
        $p = Start-Process (Join-Path $PWD "$flavor/SakhrDictionaryRevive.exe") -ArgumentList $arg -PassThru
        if (!$p.WaitForExit(90000)) {Stop-Process -Id $p.Id -Force; throw "Capture timed out: $file"}
        if ($p.ExitCode -ne 0 -or !(Test-Path $file)) {
          Get-Content (Join-Path $state 'crash.log') -ErrorAction SilentlyContinue
          throw "Capture failed: $file exit=$($p.ExitCode)"
        }
      }
    }
  }
}
# A PNG existing is only capture success. Actual pixels must still be reviewed.
Get-ChildItem evidence | Select-Object Name,Length | Format-Table | Out-String | Set-Content evidence/captures.txt
