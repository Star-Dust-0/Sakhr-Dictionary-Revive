$ErrorActionPreference = 'Stop'
# Always pack the freshly built application, never the archived payload.
if (!(Test-Path publish/SakhrDictionaryRevive.exe)) { throw 'Fresh application publish required' }
Compress-Archive -Path publish/* -DestinationPath installer/payload.zip -Force
dotnet publish installer/Installer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o setup
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed' }
