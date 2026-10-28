# Sakhr Dictionary Revive

A Windows English-Arabic dictionary with a native WinUI 3 interface, Arabic and English UI, light/dark themes, Mica/Acrylic backdrops and customizable accent colors.

brought to you by Instinct

## Features

- Offline English lookup over 59,427 dictionary entries and Arabic reverse lookup.
- Original Sakhr meanings are kept separate from optional Wiktionary results.
- Word inflections, instant suggestions and full Arabic right-to-left layout.
- Optional Wiktionary data is off by default.
- Manual update checks only. The update source is [StarsDust01/Sakhr-Dictionary-Revive](https://github.com/StarsDust01/Sakhr-Dictionary-Revive).
- Stable installer identity: new installs replace the existing app. Uninstall keeps personal settings.

## Reconstructed source

This project was reconstructed from the owner's shipped v1.0.2 application and recovered source. It is maintainable source, not a claim to recover lost comments or the exact original source text. Version 1.0.2 retains the original app identity and data. Intentional changes are the GitHub-account migration, a language-switch bug fix, and corrected installer version labels.

Validation covers 17 core assertions, 61,321 original-managed-code differential comparisons, 92 Windows captures, actual UI interactions, and install/reinstall/uninstall checks. See [verification/STATUS.md](verification/STATUS.md) for the tested matrix and limits. High-DPI, multiple monitors and every possible Windows configuration are not claimed tested.

## Build

Requires Windows x64, .NET 8 SDK and NuGet access. Windows App SDK 1.6 packages are restored automatically.

```powershell
dotnet run --project tests/CoreParity -c Release
dotnet publish src/FluentSakhrDictionary/FluentSakhrDictionary.csproj -c Release -r win-x64 -p:Platform=x64 --self-contained false -p:WindowsAppSDKSelfContained=true -o publish
./verification/build-installer.ps1
```

The installer checks for .NET 8, Visual C++ and Windows App Runtime requirements and downloads signed Microsoft installers if needed. A first install may need internet access. Dictionary use is offline.

## Data and license

Application source: MIT. Original Sakhr data was extracted from the owner's copy of the 1996 dictionary. Wiktionary-derived data: CC BY-SA 4.0, via Wiktextract/kaikki.org; see [data/LICENSE-wiktionary-ar.txt](data/LICENSE-wiktionary-ar.txt). The datasets retain their separate attribution.

Developer: Mohamed Elnaggar. Arabic recovered README is preserved as [README.ar.md](README.ar.md).

## Copy links

Repository:

```text
https://github.com/StarsDust01/Sakhr-Dictionary-Revive
```

Release page (only usable when a release has been approved and published):

```text
https://github.com/StarsDust01/Sakhr-Dictionary-Revive/releases
```
