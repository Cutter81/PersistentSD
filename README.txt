PERSISTENTSD - CONFIGURABLE SOURCE PACKAGE
========================================

Purpose
-------
PersistentSD helps configure a preferred drive letter for a selected Windows storage device and installs or updates the startup task that maintains that assignment.

The manager:
- Selects a storage device by its disk UniqueId.
- Offers preferred drive letters D: through Z: only. A:, B:, and C: are excluded in the UI and validated again before script generation and at script runtime.
- Lets you choose one script destination: %USERPROFILE%\Scripts or My Documents\Scripts.
- Looks for an existing task named "Persistent SD" or "PersistentSD" and inspects its XML action to resolve the actual script path, including scripts with nonstandard filenames.
- Checks the selected folders and the profile/Documents roots for likely existing scripts if the task action cannot be resolved.
- Backs up an existing target script and, when switching locations, the previously referenced script before installing a replacement. The old script is not automatically deleted.
- Backs up the previous scheduled-task XML when a script backup is made.
- Verifies that the task action points to the intended generated script after registration.
- Creates or updates a startup task running as SYSTEM with highest privileges.

Package files
-------------
- PersistentSDManager.cs: Windows Forms configuration and scheduled-task manager.
- Build-PersistentSD.bat: builds PersistentSD.exe using the .NET Framework C# compiler on Windows.
- PersistentSDManager.exe.manifest: requests elevation.
- RazorBlade.ico: application icon.
- PersistentSD.ps1: script template; the manager generates a configured copy in the selected user folder.
- README.txt: these instructions and safety notes.

Build and use
-------------
1. Extract the complete source package to a writable folder.
2. On Windows, run Build-PersistentSD.bat.
3. Run PersistentSD.exe as Administrator.
4. Refresh devices, then choose the intended storage device and preferred drive letter.
5. Select exactly one script destination: %USERPROFILE%\Scripts or My Documents\Scripts.
6. Click Save / Install / Update and review the confirmation prompts carefully.
7. If an existing script is replaced, the app creates a timestamped backup under PersistentSD-Backups before writing the new script. When changing locations, it also backs up the old task's script and leaves that old file in place.
8. Use Check Status to inspect the task and resolved script path. You can open Task Scheduler from the app to review the registered task.

Safety notes and limitations
---------------------------
- Device matching uses the Windows disk UniqueId. If the selected disk cannot be matched exactly and uniquely, the runtime script makes no drive-letter changes.
- Double-check the selected disk and letter before installing. If the preferred letter belongs to another non-protected partition, the script removes that access path and assigns the letter to the selected device. This may disrupt software or shortcuts that use the previous path.
- The script will not automatically bring an offline disk online or clear its read-only flag.
- The script chooses a non-system, non-boot data partition on the selected disk, preferring one already holding the selected letter, then one with an existing drive letter, then the first eligible data partition.
- The script avoids system/boot/recovery partitions and logs actions to PersistentSD.log.
- The app does not automatically delete old scripts. Remove obsolete copies manually only after you have verified the replacement works.
- The app attempts to preserve the existing task name and inspect its actual PowerShell action. If it cannot safely resolve a script path, review Task Scheduler before proceeding.
- If script writing or task registration fails after a backup, the backup is retained for recovery. Do not assume the app has performed an automatic rollback.
- The task is registered to run at startup as SYSTEM. The generated script must remain accessible at the selected path.
- UI preferences are stored under %LOCALAPPDATA%\PersistentSD\preferences.txt.
- The current window layout is 691 pixels wide, with the device selector/dropdown aligned to the same right edge as the status area and Task Scheduler button.
- This source package has not been compiled or exercised on a Windows host in this environment. Build and test on the target PC before relying on drive-letter reassignment. If your current installation is already working, keep it intact until you can test any rebuilt version safely.

