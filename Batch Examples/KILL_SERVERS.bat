@echo off
echo ========================================
echo   NightBlade MMO - Killing All Servers
echo ========================================
echo.

echo Listing all NightBlade processes...
tasklist | findstr /I "NightBlade.exe"
echo.

echo Terminating all NightBlade.exe processes...
taskkill /F /IM NightBlade.exe /T >nul 2>&1

if %ERRORLEVEL% == 0 (
    echo SUCCESS: All servers killed!
) else (
    echo No NightBlade.exe processes found.
)

echo.
echo Waiting 1 second...
timeout /t 1 /nobreak >nul

echo.
echo Double-checking...
tasklist | findstr /I "NightBlade.exe" >nul 2>&1

if %ERRORLEVEL% == 0 (
    echo WARNING: Some processes still running!
    echo Trying aggressive kill...
    wmic process where "name='NightBlade.exe'" delete >nul 2>&1
    echo Done!
) else (
    echo All clear! No processes remaining.
)

echo.
echo ========================================
echo   Complete!
echo ========================================
pause
