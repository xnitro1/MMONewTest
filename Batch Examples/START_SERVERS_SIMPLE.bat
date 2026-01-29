@echo off
echo ========================================
echo   NightBlade MMO - SIMPLE Setup
echo   (All servers in ONE window)
echo ========================================
echo.

REM Start Central + Database Server in background
echo [1/3] Starting Central + Database Server...
start /b D:\MMO\NightBlade.exe -batchmode -nographics -startCentralServer -startDatabaseServer

REM Wait 3 seconds
timeout /t 3 /nobreak >nul

REM Start Map001 Server in background
echo [2/3] Starting Map001 Server...
start /b D:\MMO\NightBlade.exe -batchmode -nographics -startMapServer -mapName Map001_new -channelId 1 -centralAddress 127.0.0.1 -centralPort 6010 -publicAddress 127.0.0.1 -mapPort 8000

REM Wait 2 seconds
timeout /t 2 /nobreak >nul

REM Start Map002 Server in background
echo [3/3] Starting Map002 Server...
start /b D:\MMO\NightBlade.exe -batchmode -nographics -startMapServer -mapName Map002_new -channelId 1 -centralAddress 127.0.0.1 -centralPort 6010 -publicAddress 127.0.0.1 -mapPort 8001

REM Start Dungeon001 Server in background
echo [3/3] Starting Dungeon001 Server...
start /b D:\MMO\NightBlade.exe -batchmode -nographics -startMapServer -mapName Dungeon001 -channelId 1 -centralAddress 127.0.0.1 -centralPort 6010 -publicAddress 127.0.0.1 -mapPort 8002

echo.
echo ========================================
echo   All servers started in background!
echo ========================================
echo.
echo Logs will appear mixed in this window.
echo Press Ctrl+C to stop all servers.
echo.
pause
