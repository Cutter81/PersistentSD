# PersistentSD

PersistentSD helps configure a preferred drive letter for a selected Windows storage device and installs or updates a startup task that maintains that assignment.

> **Important:** Review the selected disk and drive letter carefully before installing. PersistentSD can remove a preferred letter from another non-protected volume to assign it to the selected device.

## Download

For the ready-to-run Windows executable, visit the [PersistentSD v1.0 release](https://github.com/Cutter81/PersistentSD/releases/tag/v1.0).

To build from source, follow the instructions below.

## Features

- Selects a storage device by its Windows disk `UniqueId`.
- Offers preferred drive letters **D:** through **Z:**; A:, B:, and C: are excluded and validated again by the generated script.
- Lets you choose the generated script destination: `%USERPROFILE%\Scripts` or `My Documents\Scripts`.
- Checks existing scheduled tasks named `Persistent SD` or `PersistentSD` and inspects their actions to resolve the actual script path.
- Backs up an existing target script and, when changing locations, the previously referenced script before installing a replacement.
- Saves the previous scheduled-task XML when a script backup is made.
- Verifies that the registered task action points to the intended generated script.
- Creates or updates a startup task running as **SYSTEM** with highest privileges.
- Logs runtime actions to `PersistentSD.log`.

## Build from source

### Requirements

- Windows
- .NET Framework C# compiler (`csc.exe`), typically included with the .NET Framework installation
- The complete source package

### Steps

1. Download or clone this repository and extract it to a writable folder.
2. Run `Build-PersistentSD.bat` on Windows.
3. After a successful build, run `PersistentSD.exe` as Administrator.
4. Click **Refresh Devices**, then choose the intended storage device and preferred drive letter.
5. Choose exactly one script destination.
6. Click **Save / Install / Update** and carefully review the confirmation prompts.
7. Use **Check Status** to inspect the task and resolved script path. You can also open Task Scheduler from the app to review the registered task.

Keep `PersistentSD.ps1` beside the application as its generation template. The manager writes a configured copy to the selected Scripts folder.

## Safety notes and limitations

- **Verify the disk and letter before installing.** If the preferred letter belongs to another non-protected partition, the script removes that access path and assigns the letter to the selected device. This can disrupt software or shortcuts that rely on the previous path.
- Device matching uses the Windows disk `UniqueId`. If the selected disk cannot be matched exactly and uniquely, the runtime script makes no drive-letter changes.
- The script does not automatically bring an offline disk online or clear its read-only flag.
- It chooses a non-system, non-boot data partition on the selected disk, preferring one already holding the selected letter, then one with an existing drive letter, then the first eligible data partition.
- The script avoids system, boot, and recovery partitions.
- Existing scripts are not automatically deleted. When changing locations, the old script is backed up and left in place; remove obsolete copies manually only after verifying the replacement works.
- If script writing or task registration fails after a backup, the backup is retained. Do not assume an automatic rollback has occurred.
- If the app cannot safely resolve an existing script path, review Task Scheduler before proceeding.
- The startup task runs as SYSTEM, so the generated script must remain accessible at the selected path.
- Preferences are stored under `%LOCALAPPDATA%\PersistentSD\preferences.txt`.
- The current source package has not been compiled or exercised on a Windows host in the development environment. Build and test on the target PC before relying on drive-letter reassignment. If your current installation is already working, keep it intact until you can safely test a rebuilt version.

## Repository contents

- `PersistentSDManager.cs` — Windows Forms configuration and scheduled-task manager
- `Build-PersistentSD.bat` — build script
- `PersistentSDManager.exe.manifest` — requests elevation
- `RazorBlade.ico` — application icon
- `PersistentSD.ps1` — script template
- `README.txt` — detailed companion instructions and safety notes

## License

See [LICENSE](LICENSE).
