#!/usr/bin/env bash
set -euo pipefail
# Requires .NET 8 SDK. Original methods are unmodified; only Windows bootstrap is bypassed.
dotnet run --project tests/CoreParity -c Release
mkdir -p .verification-original
# Preserve the original app archive independently. The checked-in original DLL is enough for this core comparison.
dotnet run --project tests/ExtractCore -c Release -- verification/original-v1.0.2/SakhrDictionaryRevive.dll .verification-original/SakhrDictionaryRevive.dll
mkdir -p tests/Differential/bin/Release/net8.0/data
cp data/* tests/Differential/bin/Release/net8.0/data/
dotnet run --project tests/Differential -c Release -- "$PWD/.verification-original"
