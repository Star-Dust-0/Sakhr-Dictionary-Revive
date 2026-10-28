$ErrorActionPreference='Stop'
dotnet publish src/FluentSakhrDictionary/FluentSakhrDictionary.csproj -c Release -r win-x64 -p:Platform=x64 --self-contained true -p:WindowsAppSDKSelfContained=true -p:WindowsAppSdkBootstrapInitialize=false -p:DebugType=none -o publish
if($LASTEXITCODE -ne 0){throw 'Publish failed'}
$vs=& "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe" -latest -property installationPath
$crt=Get-ChildItem "$vs/VC/Redist/MSVC/*/x64/Microsoft.VC*.CRT" -Directory|Sort-Object FullName -Descending|Select-Object -First 1
if(!$crt){throw 'VC app-local CRT missing'}
Copy-Item "$($crt.FullName)/*.dll" publish -Force
$config=Get-Content publish/SakhrDictionaryRevive.runtimeconfig.json -Raw|ConvertFrom-Json
if($config.runtimeOptions.framework -or $config.runtimeOptions.frameworks){throw 'Shared .NET dependency remains'}
foreach($file in @('coreclr.dll','hostfxr.dll','Microsoft.WinUI.dll','Microsoft.WindowsAppRuntime.dll','vcruntime140.dll','msvcp140.dll')){if(!(Test-Path "publish/$file")){throw "Missing local dependency $file"}}
Copy-Item LICENSE publish/LICENSE.txt
Copy-Item data/LICENSE-wiktionary-ar.txt publish/LICENSE-wiktionary-ar.txt
@'
Sakhr Dictionary Revive 1.0.2 - x64
Extract all files together before launching SakhrDictionaryRevive.exe.
.NET, Windows App SDK and VC runtime files are bundled.
The installer installs for the current user without administrator rights.
brought to you by Instinct - https://instinct.com
'@|Set-Content publish/README.txt -Encoding utf8
# Use the recovered per-user Inno spec with root-relative paths as originally generated.
Copy-Item installer/recovered-per-user.iss self-contained.iss
& "${env:ProgramFiles(x86)}/Inno Setup 6/ISCC.exe" self-contained.iss
if($LASTEXITCODE -ne 0){throw 'Inno installer build failed'}
Compress-Archive -Path publish/* -DestinationPath SakhrDictionaryRevive-v1.0.2-portable-win-x64.zip -Force
