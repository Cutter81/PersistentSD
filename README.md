<div align="center">

# PersistentSD

**Keep your chosen drive letter assigned to the storage device you want.**

A lightweight Windows utility that configures a preferred drive letter for a selected storage device and attempts to maintain that assignment at startup.

<img src="PersistentSD.png" alt="PersistentSD application window showing scheduled-task status" width="650">

[**Download PersistentSD**](https://github.com/Cutter81/PersistentSD/releases/latest) · [View all releases](https://github.com/Cutter81/PersistentSD/releases) · [Report a problem](https://github.com/Cutter81/PersistentSD/issues)

</div>

> [!IMPORTANT]
> **Drive-letter safety:** If another partition already owns the preferred letter, PersistentSD logs the conflict and leaves that partition and its drive letter untouched. It does not forcibly take the letter away. Double-check the selected disk and preferred letter before installing.

> [!WARNING]
> **Runtime validation is incomplete.** Automated checks confirm PowerShell syntax, C# compilation, and ZIP packaging. They do not prove that installation, startup, reconnect handling, or real drive-letter assignment works correctly on every Windows system. Review [Testing status](#testing-status) before relying on the utility.

## What PersistentSD does

PersistentSD provides a Windows Forms interface for selecting a storage disk and preferred drive letter. It saves your configuration and creates a PowerShell script launched by a scheduled task at startup under the **SYSTEM** account.

The generated script identifies the selected device using its Windows disk `UniqueId`, rather than relying only on its current drive letter. The manager can install or update the task, inspect basic task status, open Task Scheduler, and remove the task.

## Features

- **Select a storage device** from the detected Windows disks.
- **Choose a preferred drive letter** from D: through Z:.
- **Configure startup handling** with a scheduled task running as SYSTEM.
- **Inspect task status** and check whether the configured script path can be resolved. This is not a complete runtime health check.
- **Manage the scheduled task** from the application.
- **Retain recovery evidence** through backups of replaced scripts/task XML and runtime logs.

## Download and install

1. Open the [latest PersistentSD release](https://github.com/Cutter81/PersistentSD/releases/latest).
2. Download `PersistentSD-v1.1.zip` (or the ZIP attached to the version you intend to use).
3. Extract the ZIP into a folder you can keep in place.
4. Keep `PersistentSD.exe` and `PersistentSD.ps1` together in that folder.
5. Run `PersistentSD.exe` as **Administrator** and read each confirmation prompt carefully.

The release ZIP includes the application, PowerShell template, README, and license. You do not need to download the EXE or script separately. Do not move or delete the script after installing the task unless you update or remove the task first.

## Build from source

**Requirements:** Windows and the .NET Framework C# compiler (`csc.exe`).

1. Clone or download this repository.
2. Keep `PersistentSD.ps1` beside the source and build files.
3. Run `Build-PersistentSD.bat` on Windows.
4. After a successful build, launch `PersistentSD.exe` as **Administrator**.

The build script searches standard 64-bit and 32-bit .NET Framework compiler locations. GitHub Actions also compiles and packages the app, but that automated build does not exercise actual storage changes.

## How to use it

1. Open PersistentSD as Administrator.
2. Refresh the device list and select the intended storage device.
3. Select a preferred drive letter from **D:** through **Z:**.
4. Choose one of the offered script folders.
5. Select **Save / Install / Update** and review the confirmation prompts.
6. Use **Check Status** to inspect the registered task and its configured script path.
7. Open Task Scheduler from the app if you want to inspect the task directly.

## Safety and behavior

| Area | What to know |
| --- | --- |
| **Device matching** | The script matches the selected disk using its Windows `UniqueId`. If it cannot find exactly one matching disk, it should make no drive-letter changes. |
| **Partition selection** | System, boot, recovery, and certain reserved partition types are intended to be excluded. Among eligible data partitions, the script prefers one already using the selected letter, then one with an existing drive letter, then the first eligible partition. |
| **Protected partitions** | Protected-partition handling is intended as a safeguard, not a substitute for validating unusual disk layouts. |
| **Letter conflicts** | If another partition owns the preferred letter, the script leaves that volume untouched and retries later; it does not remove the other partition's letter. |
| **Backups** | Existing target scripts and scheduled-task XML are backed up before replacement when the manager can identify and read them. Backups are recovery copies, not an automatic rollback mechanism. |
| **Failed updates** | If task registration fails, the manager attempts to restore the previous script or remove a newly created script and does not save new preferences. Recovery itself can fail. If registration succeeds but later verification or preference saving fails, inspect the warning, Task Scheduler, script path, and backups manually. |
| **Old files** | Previous scripts are not automatically deleted. Verify the new setup before manually removing old copies. |
| **Disk state** | The script does not automatically bring an offline disk online or clear its read-only flag. |
| **Logging** | Runtime activity is written to `PersistentSD.log` beside the generated script, if that folder is writable. |

Preferences are stored at `%LOCALAPPDATA%\PersistentSD\preferences.txt`. The startup task runs as **SYSTEM**, so the generated script must remain at the registered path and be accessible to that account. The task uses the selected device ID and drive letter embedded in the generated script; the user-profile preferences file is not the runtime task's source of truth.

## Testing status

The automated workflow checks PowerShell template syntax, compiles the C# application, and creates a ZIP. It does **not** launch the GUI, install/remove a task, or validate actual drive-letter assignment, startup behavior, or device reconnects.

Those behaviors remain unverified until tested on Windows using the exact release ZIP. Follow the [manual Windows test plan](TESTING.md), record skipped or failed checks, and do not treat a successful build as proof of runtime correctness.

## Project files

| File | Purpose |
| --- | --- |
| `PersistentSDManager.cs` | Windows Forms interface and scheduled-task management |
| `Build-PersistentSD.bat` | Builds the Windows application |
| `PersistentSDManager.exe.manifest` | Requests administrator privileges |
| `RazorBlade.ico` | Application icon |
| `PersistentSD.ps1` | PowerShell runtime script template |
| `PersistentSD.png` | Application screenshot shown above |
| `TESTING.md` | Manual Windows validation checklist |
| `LICENSE` | MIT license |

## Feedback

Found a bug or have a suggestion? [Open an issue on GitHub](https://github.com/Cutter81/PersistentSD/issues).

---

<div align="center">

*PersistentSD — a small utility for keeping storage paths predictable.*

</div>
