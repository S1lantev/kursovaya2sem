@echo off
setlocal
set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (echo Compiler not found. Install .NET Framework 4.8. & exit /b 1)
if not exist "%~dp0src\DetailingCourse\bin" mkdir "%~dp0src\DetailingCourse\bin"
"%CSC%" /nologo /target:exe /out:"%~dp0src\DetailingCourse\bin\DetailingCourse.exe" /reference:System.Runtime.Serialization.dll "%~dp0src\src\DetailingCourse\*.cs"
exit /b %errorlevel%
