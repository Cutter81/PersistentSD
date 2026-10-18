@echo off
setlocal
cd /d "%~dp0"

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"

if not exist "%CSC%" goto NoCompiler

"%CSC%" /nologo /target:winexe /platform:anycpu /optimize+ /win32icon:"RazorBlade.ico" /win32manifest:"PersistentSDManager.exe.manifest" /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /out:"PersistentSD.exe" "PersistentSDManager.cs"

if errorlevel 1 goto BuildFailed

echo.
echo Build successful: PersistentSD.exe
pause
exit /b 0

:NoCompiler
echo ERROR: .NET Framework C# compiler csc.exe was not found.
pause
exit /b 1

:BuildFailed
echo.
echo Build failed. See compiler errors above.
pause
exit /b 1
