# v1.0.2 parity summary

Reference archive SHA-256: 3ac76a710fbd8f8801d9ee20a159ed4e320ce91d0f7185ea10acf65abcddb1ba.

Successful Windows validation: https://github.com/StarsDust01/Sakhr-Dictionary-Revive/actions/runs/40757481386

- App and archived-backup installer publish: passed. Later published packaging recovery and its new validation are described below.
- 17 core assertions and 61,321 original-IL differential comparisons: passed.
- 59,427 exact English entries: every row checked.
- 92 screenshot captures: original/reconstructed matrix, both languages, Light/Dark, 800x600/1600x900, Mica/MicaAlt/Acrylic, installer states.
- Actual mouse-based Arabic/English switching, suggestions/input/selection/navigation, single instance, theme/backdrop, Wiktionary persistence, custom accent dialog/reset: passed.
- Archived-backup installer: install/reinstall stable identity, registry metadata and shortcuts; actual uninstall passed.
- Updater against private new repository: graceful failure, no token. Public release lookup remains a post-publication check.

Intentional app differences: new StarsDust01 links; ignore transient empty language selection rather than save system language; installer label corrected to 1.0.2. Data and app identity preserved.

Limits: high-DPI, multiple monitors, keyboard-only accessibility and all possible Windows configurations are not fully tested. Build/testing does not establish those claims.

## Final published packaging recovery

The archived v1.0.2 backup contained an older framework-dependent/admin installer. Historical October 11 records show the later published release instead used fully bundled runtimes, per-user Inno Setup and three clean-named packages. The recovered per-user Inno spec preserves AppId SakhrDictionaryRevive-SelfContained and its LocalAppData install path. It is restored in installer/recovered-per-user.iss.

The original English README was recovered from October 9, not rewritten from scratch, and updated for final packaging/account migration/current validation. Original box photo provenance and exact crops were recovered, but image bytes remain unavailable. Documentation screenshots are explicitly identified as new captures.

Packaging verification is in progress: app-local .NET/WinUI/Windows App Runtime loading without the runner user's shared Windows App Runtime, per-user install/reinstall, installed screenshot, uninstall, and full-offline package signature/checksum/extraction tests. This section is not a claim that these checks have passed yet.

Current packaging run: https://github.com/StarsDust01/Sakhr-Dictionary-Revive/actions/runs/40760702820
