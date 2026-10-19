# PersistentSD Manual Test Plan

Use this checklist to validate a build on a disposable test setup before relying on it. GitHub Actions checks syntax, compilation, and packaging; it does **not** execute these Windows storage-behavior tests.

## Before testing

- [ ] Use a test PC or disposable Windows installation where possible.
- [ ] Back up important data and record current drive-letter assignments.
- [ ] Do not use a disk containing irreplaceable data for destructive or ambiguous test cases.
- [ ] Run the manager elevated. Keep the release ZIP and build artifacts separate from any existing installed copy.
- [ ] Record the Windows version, release/tag or commit, test result, and relevant `PersistentSD.log` entries.
- [ ] Confirm you can recover from a failed test before testing installation or drive-letter changes.

## Manager and configuration

- [ ] Launch the manager as Administrator.
- [ ] Refresh the disk list; confirm device names, sizes, and IDs are understandable.
- [ ] Save a device selection and preferred letter, close the manager, reopen it, and confirm the selection persists.
- [ ] Confirm the script-location preference persists after closing/reopening.
- [ ] Confirm the UI rejects missing devices, missing letters, and unsupported letters.
- [ ] Confirm Check Status clearly reports a missing task and an unresolved script path.
- [ ] Confirm Remove Task requires confirmation and leaves script files/configuration intact as documented.

## Installation, update, and recovery

- [ ] Install with no existing task and a valid destination.
- [ ] Confirm the task exists, its action points to the generated script, and it runs as SYSTEM at startup.
- [ ] Update an existing task in the same destination; verify a backup of the old script and task XML is created.
- [ ] Update an existing task whose script is in the other supported destination; verify the prompt and backups are correct.
- [ ] Test an existing task whose action cannot be parsed or whose script is missing. Confirm the app does not silently report it as healthy.
- [ ] Test a read-only/unwritable destination and a task-registration failure. Confirm new preferences are not saved on registration failure and the manager attempts to restore the prior script or remove a newly created script.
- [ ] Where feasible, test a failure after task registration succeeds, such as a preference-save or script-path verification failure. Confirm the warning explains that the task may already exist.
- [ ] If recovery fails, capture the exact error and verify the task, script path, and backup folder manually.
- [ ] Confirm backups are readable and previous scripts are not deleted automatically.
- [ ] Remove the task and confirm its effect is limited to the task, without unexpectedly deleting scripts or preferences.

## Drive-letter safety (disposable test disks only)

- [ ] With the preferred letter free, confirm the intended eligible data partition receives it.
- [ ] With the preferred letter owned by a different volume, confirm that volume keeps its letter and PersistentSD logs the conflict.
- [ ] Confirm system, boot, recovery, and reserved partitions are not selected for reassignment.
- [ ] Test the selected disk offline; confirm PersistentSD does not bring it online automatically.
- [ ] Disconnect and reconnect the selected removable disk; confirm the arrival watcher and periodic retry behave as expected.
- [ ] Start with the selected disk absent, then connect it later.
- [ ] Test a disk with multiple eligible partitions and verify the selected partition follows the documented preference order.
- [ ] Test a missing, duplicate, or changed disk UniqueId; confirm no drive-letter changes occur unless exactly one disk matches.
- [ ] Inspect logs for useful error messages when storage queries or assignment attempts fail.
- [ ] If practical, simulate watcher-registration failure and confirm the task stays running and falls back to periodic retries rather than exiting.

## Release package and documentation

- [ ] Confirm the build workflow succeeds on the exact commit intended for release.
- [ ] Download the asset from the published release page, not just the Actions artifact.
- [ ] Extract the release ZIP on a clean test account or test machine.
- [ ] Confirm the ZIP contains `PersistentSD.exe`, `PersistentSD.ps1`, `README.md`, and `LICENSE`, with the EXE and script together.
- [ ] Confirm the README in the ZIP describes the same version/download flow and safety limitations as the release notes.
- [ ] Test the exact ZIP intended for publication, not only an EXE built locally.
- [ ] Use a new tag/release for changed binaries; do not replace an existing version's asset.
- [ ] Release notes must distinguish successful compilation from actual Windows runtime testing.

## Reporting results

For each failed or skipped item, record the step, Windows version, exact build/release, expected result, actual result, and relevant log output. Mark tests as **passed**, **failed**, or **not run**. Do not describe runtime behavior as verified unless that exact behavior was tested.
