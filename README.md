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

## Download

Three Windows x64 packages: portable ZIP, installer, and full offline ZIP containing the installer plus optional Microsoft runtime installers. Runtime files are bundled with the app, so no separate runtime installation is needed. The installer requires administrator rights and installs into Program Files, replacing any earlier installation. Choose the package from the release page below.

```text
https://github.com/Star-Dust-0/Sakhr-Dictionary-Revive/releases
```

## The original

The box of the original **Sakhr Dictionary, MSX Al-Alamiah edition (1987)** -
the ancestor of the 1996 Windows version this project revives, and the
dictionary whole generations grew up with. Photos from the developer's own
copy.

| Front | Back |
| --- | --- |
| <img src="docs/box-front.jpg" width="300" alt="Front of original 1987 MSX Al-Alamiah Sakhr box"> | <img src="docs/box-back.jpg" width="300" alt="Back of original 1987 MSX Al-Alamiah Sakhr box"> |

## The data

`data/dictionary.jsonl` holds the full extracted dataset: 59,427 records
aligned 1:1 with the original program's English word list, with the Arabic
meanings captured from the 1996 program itself. 45,693 lemmas resolve to
meanings; the rest are rare inflections and proper nouns absent from the
original program's own search index.

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

Requires Windows and the .NET 8 SDK. NuGet restores Windows App SDK 1.6.

## Credits

Developer: Mohamed Elnaggar

**[brought to you by Instinct](https://instinct.com)**

## License

MIT

## Repository

[github.com/Star-Dust-0/Sakhr-Dictionary-Revive](https://github.com/Star-Dust-0/Sakhr-Dictionary-Revive)
