$ErrorActionPreference = 'Stop'
dotnet run --project tests/CoreParity -c Release
if ($LASTEXITCODE -ne 0) { throw 'Core checks failed' }
# Windows can load the original assembly unchanged. No PE conversion is needed.
# Use reference dependencies in original-run, prepared by run-parity.ps1.
New-Item -ItemType Directory -Force tests/Differential/bin/Release/net8.0/data | Out-Null
Copy-Item data/* tests/Differential/bin/Release/net8.0/data/ -Force
dotnet run --project tests/Differential -c Release -- "$PWD/original-run"
if ($LASTEXITCODE -ne 0) { throw 'Differential checks failed' }
