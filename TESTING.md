# PersistentSD Manual Test Plan

This checklist is for validating a build on a disposable test setup before publishing a release. The GitHub Actions build verifies compilation and packaging; it does **not** execute these Windows storage-behavior tests.

## Before testing

- Use a test PC or disposable Windows installation where possible.
- Back up important data and record current drive-letter assignments.
- Do not use a disk containing irreplaceable data for destructive or ambiguous test cases.
- Run the manager elevated, and keep the release ZIP and source/build artifact separate from any existing installed copy.
- Record Windows version, build commit, test result, and relevant PersistentSD.log entries.

## Manager and configuration

- [ ] Launch the manager as Administrator.
- [ ] Refresh the disk list; confirm device names, sizes, and IDs are understandable.
- [ ] Confirm a saved device selection and preferred letter survive closing/reopening the manager.
- [ ] Confirm the profile/Documents script-location preference survives closing/reopening.
- [ ] Confirm the UI rejects missing devices, missing letters, and unsupported letters.
- [ ] Confirm Check Status clearly reports missing tasks and unresolved script paths.
- [ ] Confirm Remove Task requires confirmation and leaves scripts/configuration intact.

## Installation and recovery

- [ ] Install with no existing task and a valid destination.
- [ ] Confirm the task exists, its action points to the generated script, and it runs as SYSTEM at startup.
- [ ] Update an existing task in the same destination; verify a backup of the old script and task XML is created.
- [ ] Update an existing task whose script is in the other supported destination; verify the prompt and backups are correct.
- [ ] Test with an existing task whose action cannot be parsed or whose script is missing. Confirm the app does not silently claim the task is healthy.
- [ ] Test a read-only/unwritable destination and a task-registration failure. Confirm preferences are not saved on registration failure and the manager attempts to restore the prior script or remove a newly created script; record any recovery failure.
- [ ] Confirm backups are readable and no previous script is deleted automatically.

## Drive-letter safety (use disposable test disks)

- [ ] With the preferred letter free, confirm the intended eligible data partition receives it.
- [ ] With the preferred letter owned by a different volume, confirm that volume keeps its letter and PersistentSD logs the conflict.
- [ ] Confirm system, boot, recovery, and reserved partitions are not selected for reassignment.
- [ ] Test the selected disk offline; confirm PersistentSD does not bring it online automatically.
- [ ] Disconnect/reconnect the selected removable disk and confirm the arrival watcher and periodic retry behave as expected.
- [ ] Test a disk that is absent at startup and connected later.
- [ ] Test a disk with multiple eligible partitions and verify the chosen partition matches the documented selection order.
- [ ] Test a missing, duplicate, or changed disk UniqueId; confirm no drive-letter changes occur unless exactly one disk matches.
- [ ] Inspect logs for useful error messages when storage queries or assignment attempts fail.

## Release checks

- [ ] Confirm the build workflow succeeds on the exact commit intended for release.
- [ ] Download and extract the packaged ZIP on a clean test account or test machine.
- [ ] Confirm the ZIP contains the EXE, PowerShell template, README, and license, with the EXE and template together.
- [ ] Test the exact ZIP intended for publication, not only an EXE built locally.
- [ ] Publish a new version tag/release for changed binaries; do not overwrite an existing version's asset.
- [ ] Update release notes to distinguish successful compilation from runtime testing.

## Reporting results

For each failed or skipped item, record the step, Windows version, expected result, actual result, and relevant log output. Do not describe runtime behavior as verified unless the corresponding test was actually performed.
