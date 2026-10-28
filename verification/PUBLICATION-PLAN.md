# Prepared changes awaiting owner review

No main, visibility or release changes have been made.

`release-ready` is a new clean source commit based on recovered main (802e459), not a merge of reconstruction history. It includes the same tested app/installer source as the successful reconstruction run plus final documentation and a safe build-only workflow. It excludes original-binary fixtures, raw decompiler reference snapshots, and stale recovery data chunks.

After explicit go:

1. Fast-forward main to release-ready. Existing recovered main history remains intact; no force-push of main is needed.
2. Remove remote branches reverse-engineered-v1.0.2 and fixtures, after checking no newer work exists. The reconstruction branch contains earlier commits with original app-reference binaries, so it must not remain a public ref. Remove release-ready only after main points to its final source.
3. Repeat all-ref object scan after remote cleanup; do not make public if any sensitive material remains. Deleting refs does not promise purging GitHub's cached unreachable objects; obtain provider support if private material requiring purge is discovered.
4. Make repo public only within the approved scope.
5. Publish the approved v1.0.2 release, with assets named SakhrDictionaryRevive-v1.0.2-Setup-win-x64.exe and SakhrDictionaryRevive-v1.0.2-Portable-win-x64.zip, and an English changelog. No existing releases are removed.
6. Run and screenshot in-app update check against the now-public releases API, then report its actual result. Until publication this check is only verified to fail gracefully for a private repo.

Removed only within the prepared branch, still recoverable from original main/WIP until approved:

- recovered-loose-data-files/00.txt through 21.txt: incomplete/stale data fragments superseded by byte-for-byte final datasets.
- reverse-engineered/v1.0.2 raw decompiler snapshots: reference-only, duplicate stale compiler-generated source.
- original-v1.0.2 reference binaries: never included in this branch.
- old debug.yml, dump-matrix.yml, dump.yml, probe.yml, extract-inflections.yml, extract-translations.yml, release.yml and recovery-verify.yml workflows: stale diagnostics, old paths, automatic release or private-fixture dependency. Replaced by one read-only-permission build/test workflow without release effects.

Original recovered source-provenance records and extraction utilities remain; no private credentials or runtime records belong in final source.
