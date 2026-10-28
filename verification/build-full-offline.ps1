$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force offline/runtimes|Out-Null
Copy-Item out/SakhrDictionaryRevive-v1.0.2-installer-win-x64.exe offline/Install.exe
$downloads=@(
 @{url='https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/8.0.33/windowsdesktop-runtime-8.0.33-win-x64.exe';name='windowsdesktop-runtime-x64.exe';sha512='b622106783dee5e1ce993ec849ecd43fa147c6230daecf910348f6adcd28f63abf39cf6db832a6a2684941c19a9321816e9cbc9dbe8c4a8bb2d7e46487ab3890'},
 @{url='https://aka.ms/windowsappsdk/1.6/1.6.250602001/windowsappruntimeinstall-x64.exe';name='windowsappruntimeinstall-x64.exe';sha512=''},
 @{url='https://aka.ms/vc14/vc_redist.x64.exe';name='vc_redist.x64.exe';sha512=''}
)
foreach($f in $downloads){
 $path="offline/runtimes/$($f.name)"
 Invoke-WebRequest $f.url -OutFile $path
 $sig=Get-AuthenticodeSignature $path
 if($sig.Status -ne 'Valid' -or $sig.SignerCertificate.Subject -notmatch 'Microsoft Corporation'){throw "Invalid Microsoft signature: $path"}
 if($f.sha512 -and (Get-FileHash $path -Algorithm SHA512).Hash -ne $f.sha512){throw '.NET checksum mismatch'}
}
@'
Full offline kit
Run Install.exe to install the app without internet or administrator rights.
The app includes .NET, Windows App SDK and VC runtime files. No separate runtime installation is required.
The runtimes folder additionally contains original signed Microsoft offline installers for .NET Desktop Runtime 8, Windows App Runtime 1.6 and Visual C++ x64.
These optional system installers can require administrator rights. They are not needed to run this app.
'@|Set-Content offline/README.txt -Encoding utf8
Get-ChildItem offline -Recurse -File|Get-FileHash -Algorithm SHA256|Select-Object Path,Hash|ConvertTo-Json|Set-Content offline/SHA256.json
Compress-Archive offline/* SakhrDictionaryRevive-v1.0.2-full-offline-win-x64.zip -Force
Expand-Archive SakhrDictionaryRevive-v1.0.2-full-offline-win-x64.zip offline-test -Force
$p=Start-Process ./offline-test/Install.exe -ArgumentList '/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART' -Wait -PassThru
if($p.ExitCode -ne 0){throw 'Offline kit installation failed'}
$root=Join-Path $env:LOCALAPPDATA 'Programs\SakhrDictionaryRevive-SelfContained'
$p=Start-Process "$root/SakhrDictionaryRevive.exe" -ArgumentList '--screenshot',"$PWD/evidence/offline-installed.png",'--shot-word','CAT' -PassThru
if(!$p.WaitForExit(90000)){Stop-Process -Id $p.Id -Force;throw 'Offline installed capture timed out'}
if(!(Test-Path evidence/offline-installed.png)){throw 'Offline installed capture missing'}
$p=Start-Process "$root/unins000.exe" -ArgumentList '/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART' -PassThru -Wait
if($p.ExitCode -ne 0 -or(Test-Path "$root/SakhrDictionaryRevive.exe")){throw 'Offline kit uninstall failed'}
'PASS full-offline archive extraction, Microsoft signatures, .NET SHA512, install, launch and uninstall'|Set-Content evidence/offline-tests.txt
