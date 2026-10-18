PERSISTENT SD - SOURCE PACKAGE

Contents
- PersistentSDManager.cs: Windows Forms task manager source
- Build-PersistentSD.bat: builds PersistentSD.exe on Windows with the .NET Framework C# compiler
- PersistentSDManager.exe.manifest: requests elevation because Task Scheduler operations require administrator rights
- RazorBlade.ico: copied from the existing Razor Blade-branded utility package; used as the actual executable and window icon
- RDrivePersistentSD.ps1: original working drive-letter watcher, copied without edits
- Persistent SD.xml: supplied task definition reference

Build
1. Extract the entire folder to a writable location.
2. Run Build-PersistentSD.bat on Windows.
3. Run PersistentSD.exe as administrator (it requests elevation automatically).
4. Confirm the script path, then choose Install / Update Task.

Task behavior
- Exact task name: Persistent SD
- Boot trigger, SYSTEM account, highest privileges, hidden PowerShell process
- Existing task: Yes overwrites, No leaves the existing task untouched, Cancel does nothing
- Remove Task deletes only the scheduled task; it does not delete the script.
- Script path is selectable so it can match the actual installation location.

Important
- This package does not execute the task or alter any drive letters on the machine where it is built.
- Windows Task Scheduler/runtime behavior must be tested on the target Windows PC.
- The supplied original script is preserved as-is. Review its drive-letter conflict behavior before enabling it on a machine where R: may be in active use.
