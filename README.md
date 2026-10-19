<div align="center">

# PersistentSD

**Keep your chosen drive letter assigned to the storage device you want.**

A lightweight Windows utility that configures a preferred drive letter and maintains it with a scheduled task at startup.

[Download v1.0](https://github.com/Cutter81/PersistentSD/releases/tag/v1.0) · [Report a problem](https://github.com/Cutter81/PersistentSD/issues)

</div>

> [!WARNING]
> **Read before installing:** PersistentSD may remove the selected drive letter from another non-protected volume to assign it to your chosen device. That can break shortcuts, applications, or other paths that depend on the previous letter. Double-check the disk and letter before confirming any changes.

## At a glance

- **Choose the device** from the detected Windows storage disks.
- **Choose a preferred letter** from D: to Z:.
- **Install or update** a startup task that runs as SYSTEM.
- **Check status** to inspect the scheduled task and the script it runs.
- **Keep a safety trail** with backups of scripts being replaced and logs from the runtime script.

## Get started

### Option 1 — Download the release

1. Open the [PersistentSD v1.0 release page](https://github.com/Cutter81/PersistentSD/releases/tag/v1.0).
2. Download `PersistentSD.exe` and the accompanying release information.
3. Keep the application and its `PersistentSD.ps1` template together.
4. Run the manager as **Administrator**.

### Option 2 — Build from source

**Requirements:** Windows and the .NET Framework C# compiler (`csc.exe`).

1. Download or clone this repository.
2. Keep `PersistentSD.ps1` beside the source and build files.
3. Run `Build-PersistentSD.bat` on Windows.
4. After a successful build, launch `PersistentSD.exe` as **Administrator**.

The build script looks for the .NET Framework compiler in the standard 64-bit and 32-bit Windows locations.

## How to use it

1. Open PersistentSD as Administrator.
2. Refresh the device list and select the intended storage device.
3. Select the preferred drive letter (**D:** through **Z:**).
4. Choose one of the offered Scripts folders.
5. Select **Save / Install / Update** and read the confirmation prompts carefully.
6. Use **Check Status** to review the registered task and script path.

Task Scheduler can also be opened from the app if you want to inspect the task directly.

## Safety and behavior

| Area | What to know |
| --- | --- |
| **Device matching** | The script matches the selected disk using its Windows `UniqueId`. If it cannot find exactly one matching disk, it makes no drive-letter changes. |
| **Protected partitions** | System, boot, and recovery partitions are excluded from reassignment. |
| **Existing drive letters** | A preferred letter may be taken from another non-protected partition. Check what currently uses that letter first. |
| **Backups** | Existing target scripts are backed up before replacement. When changing script locations, the previously referenced script is backed up where applicable. |
| **Old files** | Previous scripts are not automatically deleted. Verify the new setup before manually removing any old copies. |
| **Failures** | Backups are retained if a later step fails; do not assume the app has automatically rolled everything back. |
| **Disk state** | The script does not automatically bring an offline disk online or clear its read-only flag. |
| **Logging** | Runtime activity is written to `PersistentSD.log`. |

Preferences are stored at `%LOCALAPPDATA%\PersistentSD\preferences.txt`. The startup task runs as **SYSTEM**, so its configured script must remain at the registered path and accessible to that account.

## Project files

| File | Purpose |
| --- | --- |
| `PersistentSDManager.cs` | Windows Forms configuration interface and scheduled-task management |
| `Build-PersistentSD.bat` | Builds the Windows application |
| `PersistentSDManager.exe.manifest` | Requests administrator privileges |
| `RazorBlade.ico` | Application icon |
| `PersistentSD.ps1` | PowerShell script template used by the manager |
| `README.txt` | Additional project instructions |

## Testing status

The source package has **not been compiled or tested on a Windows machine in the development environment**. Build and verify it on your own Windows system before relying on it to manage drive letters. If an existing installation is working, leave it in place until you can safely test the rebuilt version.

## Feedback

Found a bug or have a suggestion? [Open an issue on GitHub](https://github.com/Cutter81/PersistentSD/issues).

---

<div align="center">

*PersistentSD — a small utility for keeping storage paths predictable.*

</div>
