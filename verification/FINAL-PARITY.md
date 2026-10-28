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

The original English README was recovered from October 9, not rewritten from scratch, and updated for final packaging/account migration/current validation. Original box photographs and Light/Apple and Dark/Home screenshots were re-supplied by the developer and restored inline. New verification screenshots are labeled separately. The original recovered PNG icon is centered above the heading.

Packaging verification passed: app-local .NET/WinUI/Windows App Runtime loading without the runner user's shared Windows App Runtime, per-user install/reinstall, installed screenshot, uninstall, and full-offline package signature/checksum/extraction tests. All checks below passed in the final bundled-runtime run.

Current packaging run: https://github.com/StarsDust01/Sakhr-Dictionary-Revive/actions/runs/40761647931

The final bundled-runtime run also repeated the real UI interaction regression: text entry/suggestion selection, navigation, single instance, theme/backdrop, Wiktionary persistence, custom color dialog/reset, actual Arabic/English switching, update-check completion and button re-enable. All passed. Installed and offline-installed screenshots were inspected directly and show the correct CAT result layout.

Runtime evidence shows hostfxr, coreclr, Microsoft.WinUI and Microsoft.WindowsAppRuntime loaded from the app's own publish folder after shared Windows App Runtime removal. Per-user registry metadata, clean shortcuts, reinstall and full uninstall passed. Full offline archive extraction, Microsoft Authenticode checks and .NET SHA-512 passed.

Release artifact SHA-256:

- installer: 4ad9ecffebfa72902427a97ff31104786a8ac04487d62aad8bb6782d4c70bdd6
- portable: ad6785c898db51a3f975b0485a2fab35782f0fb0feecc96140d9dcc0c3a27e0d
- full offline: bf237cdc8e79dccf4b8f9d732dda82ec11310ba56bc3cb2a9d34a7af26f6881b

Artifacts are taken unchanged from successful run 40761647931. Subsequent source changes before publication affect only README/icon/photo documentation, not app, installer, packaging or test code.

## Published updater and source cleanup

The exact published portable asset passed a fresh anonymous SHA-256 check and the real UI regression. The update button returned "You're up to date (v1.0.2)." Run: https://github.com/StarsDust01/Sakhr-Dictionary-Revive/actions/runs/40762793931

The working tree is one current codebase. Superseded installer source, old extraction utilities, reference-binary comparison harnesses, historical recovery notes, temporary checklist and unused generated Light/Dark screenshots were removed after owner approval. Final application source, seven runtime datasets, original icons, licenses, current Inno installer, active regression tests/workflows and README images remain. Published release assets/tags and Git history are unchanged.
