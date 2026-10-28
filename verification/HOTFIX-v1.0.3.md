# v1.0.3 installer hotfix

Owner report (2026-10-29): the recovered per-user installer put the app in a
"SakhrDictionaryRevive-SelfContained" folder, while the original releases
installed to C:\Program Files\SakhrDictionaryRevive. Owner instruction for all
his Windows apps: install into C:\Program Files\<AppName> (administrator), with
no "Self-contained" wording in anything visible.

## What the original installer did (evidence)

- Original v1.0.0-1.0.1 installer (custom single-file WPF setup, recovered from
  development records and the installed-folder backup): target
  `Program Files\SakhrDictionaryRevive`, uninstall registration
  `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SakhrDictionaryRevive`,
  DisplayName "Sakhr Dictionary Revive", clean-name shortcuts on the common
  desktop and Start Menu. v0.x-era identity: `FluentSakhrDictionary` /
  "Fluent Sakhr Dictionary".
- October 11 bundled-runtime rebuild switched to per-user Inno Setup
  (`AppId=SakhrDictionaryRevive-SelfContained`,
  `%LOCALAPPDATA%\Programs\SakhrDictionaryRevive-SelfContained`,
  `PrivilegesRequired=lowest`). The republished v1.0.2 assets kept that shape.
- In-app update check only links to the release page; elevation is handled by
  the installer itself through the normal UAC prompt, so no app change was
  needed for the Program Files move.

## Changes in 1.0.3

- New `installer/setup.iss`: admin Inno installer, `AppId=SakhrDictionaryRevive`,
  `{autopf}\SakhrDictionaryRevive`, clean shortcut names, optional "Create a
  desktop shortcut" task (checked by default, matching the original behavior of
  always creating one), version 1.0.3. The old per-user spec is removed.
- Migration at install time: silently uninstalls the per-user
  `SakhrDictionaryRevive-SelfContained` build and removes the legacy admin
  registrations (`SakhrDictionaryRevive` and v0.x `FluentSakhrDictionary`) with
  their folders and shortcuts, leaving exactly one clean Add/Remove entry.
  Personal settings in `%APPDATA%\SakhrDictionaryRevive` are never touched.
- Packaging scripts, release asset names, README and offline-kit notes updated
  (Program Files + administrator rights; no self-contained wording in any
  user-visible name).

## Verification (CI, clean windows-2022 runner)

- `test-self-contained.ps1` stages a machine with all three earlier shapes
  (real published per-user v1.0.2 installer, simulated original admin install,
  simulated v0.x install), then installs 1.0.3 and asserts: Program Files
  install, no per-user folder/registry, no legacy/ancient entries, single
  clean Add/Remove entry, clean shortcut targets, settings preserved, reinstall
  stays single-entry, installed app renders, silent uninstall works and keeps
  settings.
- `installer-wizard-capture.ps1` captures the actual wizard (welcome and the
  Additional Tasks page with the desktop-shortcut option).
- App code, datasets and portable packaging are unchanged apart from the
  version number.
