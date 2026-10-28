# Source cleanup and publication record

Main has not yet been changed. The owner made the repository public and later authorized releasing each fully verified app without another review gate. No existing release is to be deleted.

The temporary fixtures and reconstruction remote refs have been removed, together with twenty temporary Actions artifacts after local evidence preservation. Original binary references must not be reintroduced. A fresh all-ref scan is required before publishing.

`release-ready` is based on recovered main (802e459), rather than merging the temporary reconstruction history. Main can be fast-forwarded once verification is complete. Preserve existing main history.

Final restored package names:

- SakhrDictionaryRevive-v1.0.2-installer-win-x64.exe
- SakhrDictionaryRevive-v1.0.2-portable-win-x64.zip
- SakhrDictionaryRevive-v1.0.2-full-offline-win-x64.zip

The release must use these verified artifacts, an English changelog and tag v1.0.2. The live public update endpoint must then be checked. High-DPI/multi-monitor/keyboard-only testing limits remain disclosed.

Removed only within the prepared branch, still recoverable from original main/WIP in the local recovery workspace:

- recovered-loose-data-files/00.txt through 21.txt: incomplete/stale data fragments superseded by byte-for-byte final datasets.
- reverse-engineered/v1.0.2 raw decompiler snapshots: reference-only, duplicate stale compiler-generated source.
- original-v1.0.2 reference binaries: never included in this branch.
- old debug.yml, dump-matrix.yml, dump.yml, probe.yml, extract-inflections.yml, extract-translations.yml, release.yml and recovery-verify.yml workflows: stale diagnostics, old paths, automatic release or private-fixture dependency. Replaced by the recovered bundled-runtime packaging and a read-only-permission build/test workflow without release effects.

Original recovered source-provenance records and extraction utilities remain; no private credentials or runtime records belong in final source.
