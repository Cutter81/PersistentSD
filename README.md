<div align="center">

# PersistentSD

**Keep your chosen drive letter assigned to the storage device you want.**

A lightweight Windows utility designed to configure a preferred drive letter and maintain that assignment with a scheduled task at startup.

<img src="PersistentSD.png" alt="PersistentSD application window showing the scheduled task status" width="650">

[Download latest release](https://github.com/Cutter81/PersistentSD/releases) · [Report a problem](https://github.com/Cutter81/PersistentSD/issues)

</div>

> [!IMPORTANT]
> **Drive-letter safety:** The current runtime script checks whether the preferred letter is already assigned to a partition. If another partition owns it, PersistentSD logs the conflict and leaves that volume and its drive letter untouched. It does not force the reassignment. Always double-check the selected disk and preferred letter before installing.

## What PersistentSD is designed to do

PersistentSD gives you a Windows Forms interface for selecting a storage disk and the drive letter you want it to use. It saves your preferences and sets up a PowerShell script to run through a scheduled task at startup under the **SYSTEM** account, helping reapply the chosen letter to the intended disk.

The manager uses the Windows disk `UniqueId` to identify the selected device rather than relying only on its current drive letter. It also provides controls to install or update the task, check its status, open Task Scheduler, and remove the task.

## At a glance

- **Choose the device** from the detected Windows storage disks.
- **Choose a preferred letter** from D: to Z:.
- **Install or update** a startup task that runs as SYSTEM.
- **Check status** to inspect whether the task is registered and whether its script path can be resolved. This is not a complete health check of runtime drive assignment.
- **Manage the task** from the app, including opening Task Scheduler or removing the task.
- **Keep a safety trail** with backups of scripts being replaced and logs from the runtime script.

## Get started

### Option 1 — Download the release

1. Open the [PersistentSD releases page](https://github.com/Cutter81/PersistentSD/releases).
2. Download the ZIP attached to the release you intend to use.
3. Extract the ZIP to a folder you can keep in place.
4. Keep `PersistentSD.exe` and `PersistentSD.ps1` together in that extracted folder, then run the manager as **Administrator**.

The ZIP includes the application, script template, license, and README. You do not need to download the EXE or script separately.

### Option 2 — Build from source

**Requirements:** Windows and the .NET Framework C# compiler (`csc.exe`).

1. Download or clone this repository.
2. Keep `PersistentSD.ps1` beside the source and build files.
3. Run `Build-PersistentSD.bat` on Windows.
4. After a successful build, launch `PersistentSD.exe` as **Administrator**.

The build script looks for the .NET Framework compiler in the standard 64-bit and 32-bit Windows locations. GitHub Actions also compiles and packages the app, but that automated build does not exercise real disk assignment behavior.

## How to use it

1. Open PersistentSD as Administrator.
2. Refresh the device list and select the intended storage device.
3. Select the preferred drive letter (**D:** through **Z:**).
4. Choose one of the offered Scripts folders.
5. Select **Save / Install / Update** and read the confirmation prompts carefully.
6. Use **Check Status** to review whether the task and its script path can be found.

Task Scheduler can also be opened from the app if you want to inspect the task directly.

## Safety and behavior

| Area | What to know |
| --- | --- |
| **Device matching** | The script matches the selected disk using its Windows `UniqueId`. If it cannot find exactly one matching disk, it should make no drive-letter changes. |
| **Partition selection** | The script skips partitions marked as system, boot, or recovery and certain partition types. Among eligible data partitions on the selected disk, it prefers one already using the chosen letter, then one with an existing drive letter, then the first eligible partition. |
| **Protected partitions** | System, boot, and recovery partitions are intended to be excluded from reassignment. Validate unusual disk layouts before relying on the utility. |
| **Existing drive letters** | If another partition already owns the preferred letter, the runtime script leaves it untouched and retries later. It does not remove the other partition's letter. |
| **Backups** | Existing target scripts and scheduled-task XML are backed up before replacement when the manager can identify and read them. Backups are recovery copies, not an automatic rollback mechanism. |
| **Failed updates** | If task registration fails, the manager attempts to restore the previous target script (or remove a newly created one) and does not save new preferences. Recovery itself can fail; inspect the error message, Task Scheduler, and backup folder before retrying. If registration succeeds but later verification fails, inspect the task and script manually. |
| **Old files** | Previous scripts are not automatically deleted. Verify the new setup before manually removing any old copies. |
| **Disk state** | The script does not automatically bring an offline disk online or clear its read-only flag. |
| **Logging** | Runtime activity is written to `PersistentSD.log` beside the generated script, subject to the script folder being writable. |

Preferences are stored at `%LOCALAPPDATA%\PersistentSD\preferences.txt`. The startup task runs as **SYSTEM**, so its configured script must remain at the registered path and accessible to that account. The user-profile preference file is not the runtime task's source of truth; the generated script contains the selected device ID and drive letter.

## Testing status

The automated workflow verifies that the C# application compiles and that a ZIP can be packaged. It does not run the application or validate Windows storage changes. See the [manual Windows test plan](TESTING.md) for installation, failure-recovery, drive-letter conflict, and device reconnect checks. Treat behavior as unverified until the relevant tests have actually been run on a test system.

## Project files

| File | Purpose |
| --- | --- |
| `PersistentSDManager.cs` | Windows Forms configuration interface and scheduled-task management |
| `Build-PersistentSD.bat` | Builds the Windows application |
| `PersistentSDManager.exe.manifest` | Requests administrator privileges |
| `RazorBlade.ico` | Application icon |
| `PersistentSD.ps1` | PowerShell script template used by the manager |
| `PersistentSD.png` | Application screenshot displayed above |
| `TESTING.md` | Manual Windows validation checklist |

## Feedback

Found a bug or have a suggestion? [Open an issue on GitHub](https://github.com/Cutter81/PersistentSD/issues).

---

<div align="center">

*PersistentSD — a small utility for keeping storage paths predictable.*

</div>
