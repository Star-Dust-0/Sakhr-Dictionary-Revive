<p align="center"><img src="src/FluentSakhrDictionary/app-icon-256.png" width="192" height="192" alt="Sakhr Dictionary Revive icon"></p>

# Sakhr Dictionary Revive

The classic Sakhr Dictionary reborn as a modern Windows app.

## The story

In 1996, Sakhr released the Sakhr Dictionary for Windows, the successor of
an even older 1987 edition for MSX Al-Alamiah machines. Whole generations
across the Arab world grew up with it; for many of us it was the first
electronic dictionary we ever knew.

Today the dictionary comes back to life as a modern Windows app. It carries
the complete original data, extracted from the user's own copy of the 1996
program, and presents it in a native WinUI 3 (Fluent) interface, with new
capabilities the original program never had.

## Features

- **Instant search in both directions**: English -> Arabic and
  Arabic -> English, from the very first letter, fully offline.
- **The complete original data**: 59,427 English lemmas and 125,896 Arabic
  meanings from the 1996 Sakhr dictionary, with Arabic diacritics rendered
  correctly (something the original program itself could not do).
- **Inflection redirect in both languages**: type an inflected form such as
  ABANDONS, or a conjugated Arabic verb or plural, and the app takes you
  straight to the base entry with a note naming the form
  (ABANDONS <- ABANDON).
- **Wiktionary Arabic data**: 36,627 Arabic entries from English
  Wiktionary. Their meanings appear separately from the original Sakhr translation
  in clearly labeled sections - the two sources never mix.
- **Native modern interface**: Fluent design with a Mica backdrop, light
  and dark themes, and full right-to-left Arabic support.
- **Your accent color**: the default is teal (#00A6A6); pick any color you
  like in Settings.
- **Manual updates**: the app never checks for updates on its own - one
  button in About checks when you ask it to.

## Screenshots

| Light | Dark |
| --- | --- |
| ![Light theme, Apple lookup](docs/screenshots/owner-light-apple.jpg) | ![Dark theme, home](docs/screenshots/owner-dark-home.jpg) |

| Arabic search | Settings |
| --- | --- |
| ![Arabic search](docs/screenshots/app-arabic.png) | ![Settings](docs/screenshots/app-settings.png) |

The Light/Apple and Dark/Home screenshots above were supplied by the developer. The Arabic-search and Settings screenshots are new Windows verification captures of the recovered app, not the lost original documentation images.

## Download

Three Windows x64 packages: portable ZIP, per-user installer, and full offline ZIP containing the installer plus optional Microsoft runtime installers. Runtime files are bundled with the app. No separate runtime installation or administrator rights are needed for the per-user installer. Choose the package from the release page below.

```text
https://github.com/StarsDust01/Sakhr-Dictionary-Revive/releases
```

## The original

The box of the original **Sakhr Dictionary, MSX Al-Alamiah edition (1987)** -
the ancestor of the 1996 Windows version this project revives, and the
dictionary whole generations grew up with. Photos from the developer's own
copy.

| Front | Back |
| --- | --- |
| <img src="docs/box-front.jpg" width="300" alt="Front of original 1987 MSX Al-Alamiah Sakhr box"> | <img src="docs/box-back.jpg" width="300" alt="Back of original 1987 MSX Al-Alamiah Sakhr box"> |

These original box photographs were re-supplied by the developer during source recovery. The supplied copies are already tightly cropped; no generated replacement or redraw is used.

## The data

`data/dictionary.jsonl` holds the full extracted dataset: 59,427 records
aligned 1:1 with the original program's English word list, with the Arabic
meanings captured from the 1996 program itself. 45,693 lemmas resolve to
meanings; the rest are rare inflections and proper nouns absent from the
original program's own search index. The original legacy archive is not included in this recovered source.

- **English -> Arabic:** the original 1996 Sakhr dictionary data,
  extracted from the user's own copy.
- **Arabic -> English and the inflection tables:** Arabic entries and
  form-of relations from English Wiktionary, machine-extracted by
  wiktextract and published at
  [kaikki.org](https://kaikki.org/dictionary/Arabic/), licensed
  [CC BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0/) (see
  `data/LICENSE-wiktionary-ar.txt`).

The application source code is MIT licensed; the Wiktionary-derived data
ships under CC BY-SA 4.0 with attribution.

## Build from source

```
dotnet publish src/FluentSakhrDictionary/FluentSakhrDictionary.csproj -c Release -r win-x64
```

Requires Windows and the .NET 8 SDK. NuGet restores Windows App SDK 1.6. The default publish is self-contained. See verification/FINAL-PARITY.md for the tested matrix and limits.

## Credits

Developer: Mohamed Elnaggar

Contributors: app.instinct.com

brought to you by Instinct

## License

MIT

## Source recovery and validation

The English README text was recovered from the October 9 original and then updated for the October 11 bundled-runtime packaging, account migration and current verification state. Original box photos were re-supplied by the developer; screenshots and icon provenance are stated above. Current screenshots can be generated by testing, but are identified as new captures. App logic is reconstructed from the shipped v1.0.2 managed code; datasets/icons are recovered byte-for-byte. Exact lost source comments and formatting are not claimed recovered.

Repository:

```text
https://github.com/StarsDust01/Sakhr-Dictionary-Revive
```
