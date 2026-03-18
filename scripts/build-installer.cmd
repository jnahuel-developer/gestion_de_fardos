@echo off
powershell -ExecutionPolicy Bypass -File "%~dp0build-installer.ps1" %*
set EXITCODE=%ERRORLEVEL%
if not "%EXITCODE%"=="0" (
  echo.
  echo [ERROR] Fallo la generacion del instalador. Revise el mensaje anterior.
  pause
)
exit /b %EXITCODE%
