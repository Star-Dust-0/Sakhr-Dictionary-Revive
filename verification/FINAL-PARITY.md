# v1.0.2 parity summary

Reference archive SHA-256: 3ac76a710fbd8f8801d9ee20a159ed4e320ce91d0f7185ea10acf65abcddb1ba.

Successful Windows validation: https://github.com/StarsDust01/Sakhr-Dictionary-Revive/actions/runs/40757481386

- App and installer publish: passed.
- 17 core assertions and 61,321 original-IL differential comparisons: passed.
- 59,427 exact English entries: every row checked.
- 92 screenshot captures: original/reconstructed matrix, both languages, Light/Dark, 800x600/1600x900, Mica/MicaAlt/Acrylic, installer states.
- Actual mouse-based Arabic/English switching, suggestions/input/selection/navigation, single instance, theme/backdrop, Wiktionary persistence, custom accent dialog/reset: passed.
- Install/reinstall stable identity, registry metadata and shortcuts; actual uninstall: passed.
- Updater against private new repository: graceful failure, no token. Public release lookup remains a post-publication check.

Intentional differences: new StarsDust01 links; ignore transient empty language selection rather than save system language; installer label corrected to 1.0.2. Data and app identity preserved.

Limits: high-DPI, multiple monitors, keyboard-only accessibility and all possible Windows configurations are not fully tested. Build/testing does not establish those claims.
