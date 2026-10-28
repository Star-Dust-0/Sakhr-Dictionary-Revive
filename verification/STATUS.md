# Sakhr v1.0.2 reconstruction verification (WIP)

Not ready for delivery. No release, no main edits.

## Provenance

Reference: owner's `SakhrDictionaryRevive (Latest Release backup).rar` downloaded from Drive. SHA-256 `3ac76a710fbd8f8801d9ee20a159ed4e320ce91d0f7185ea10acf65abcddb1ba` matches the earlier transfer manifest. Assembly version 1.0.2.0, informational version `1.0.2+6f7b9e73361d434ecb293927be355bdd7be3c3df`.

C# reconstructed using ILSpy 9.1.0.7988 with .NET 8 and all shipped references. Including the reference assemblies was essential: missing references produced invalid async-state-machine and Span C# output. XAML compiler-generated members were removed and regenerated through XAML source. The recovered XAML was stale; several missing named controls and event connections have been restored. XAML layout parity is NOT verified yet.

All seven data files and both icon assets copied byte-for-byte from the final archive. Hashes in data-sha256.txt. Raw app-only decompile snapshot under reverse-engineered/v1.0.2 is reference material, excluded from the compiled app project.

## Completed checks

- 17 core checks pass, including concurrent loading, English and Arabic lookups, empty and missing inputs, Wiktionary on/off, inflections, normalization, exact lookup of all 59,427 dictionary rows, first-run defaults, settings persistence/corruption fallback, and localization.
- 61,321 differential comparisons pass against the ORIGINAL managed IL: exact lookup for every dictionary row, sampled prefixes/translations/inflections, and Arabic query cases with Wiktionary on/off.
- Differential execution changes only the reference PE flags for Linux loading and removes its module initializer (Windows bootstrap). Original method bodies remain unchanged. Original WinUI alignment properties are not exercised by this core harness. This is not a UI test.
- Windows WinUI publish succeeded in the branch-only workflow. Real screenshot matrix is running; success is not yet claimed.

## Still required

- Windows build, resolve any compile and XAML errors.
- Original vs reconstructed real screenshots: both UI languages, all themes, search and suggestions, empty/not found/inflection results, Settings/About, RTL caption mirroring, narrow/wide windows.
- Settings persistence, page switching, custom accent and backdrop, keyboard search, single instance, graceful update-check failure. Online old-account links are preserved as reference behavior, not declared working.
- Installer identity/upgrade/uninstall checks. Installer source reconstructed from the shipped single-file uninstaller bundle, with matching Program Files and uninstall registry identity. It compiles locally; version labels adjusted from the archived installer's stale 1.0.1 to app version 1.0.2. Installer behavior is not verified yet.
- User-PC-specific rendering or integration differences, if any, must be reported rather than marked passed.

The English README in this branch must not imply final readiness. main keeps its original README untouched.

## First real screenshot review

64 captures (32 original + 32 reconstructed) completed successfully on Windows, English/Arabic, Light/Dark, home, exact English/Arabic results, inflection, missing word, Settings, About/top and bottom. Pixel inspection found real reconstruction gaps: Arabic meaning alignment was left instead of right in English UI, home title was shortened, offline caption weight differed, About icon used low-resolution ICO, and update card lacked its border/secondary text styling. Correcting these before final validation. Passing CI and matching core output did not establish visual parity.

## Latest complete Windows pass

Run 40748666221 completed successfully. App and installer publish, 92 captures, actual UI input/suggestion selection/navigation/single-instance, theme/backdrop control changes, Wiktionary persistence, custom-color dialog opening/closing/reset, update-check completion and button re-enable all pass. Clean install and in-place reinstall, correct Program Files path and stable registry identity, both shortcuts, actual uninstaller removal of files and registry key pass. 17 core assertions and 61,321 original-IL differential comparisons pass on Windows.

Updated actual pixel pairs were inspected: English/Arabic CAT results and Arabic reverse lookup now match the original's alignment; Home and About use restored title/icon/card styling. Both languages, Light/Dark, Mica/MicaAlt/Acrylic and 800x600/1600x900 captures have been reviewed. This is evidence for the tested matrix, not a claim that every possible Windows configuration is identical. Interactive language switching, keyboard-only navigation, high-DPI and multi-monitor checks still need explicit coverage before final release.

Release is held by the owner's coordinator: decide whether to retain the original legacy update lookup (it fails gracefully against the unavailable old repository) or use the private-repository notice + releases link. An unapplied source patch is at options/private-update-notice.patch. No token is embedded. Nothing has been released or merged to main.


## Intentional migration difference

At the owner's explicit request, update-source API URL and About repository/release URLs now point to StarsDust01/Sakhr-Dictionary-Revive instead of MohamedElnaggar00. This is an intentional difference from the original app, not a parity regression. The updater is still anonymous and no token is embedded. Against a private repo it displays its error message and re-enables the check button; public visibility or a public releases repository is needed for anonymous release lookup.


## Interactive language-switch bug

Real input exposed a transient RadioButtons selection event with SelectedIndex=-1 and SelectedItem=null while changing languages. The reconstructed/original handler treated that intermediate state as system language, saved it, and rebuilt the page before the intended selection completed. Added a guard that ignores the transient null selection instead of storing system. This is a root-cause fix beyond the original behavior. Validation rerun pending.
