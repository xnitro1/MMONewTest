18:28 28.01.2026@echo off
echo ========================================
echo   NightBlade MMO - SIMPLE Setup
echo   (No CCU, keeps servers alive)
echo ========================================
echo.

REM Start Central + Database Server
echo [1/3] Starting Central + Database Server...
start "NightBlade - Central+Database" cmd /k "D:\MMO\NightBlade.exe -batchmode -nographics -startCentralServer -startDatabaseServer -spawnExePath "D:\MMO\NightBlade.exe""

REM Wait 3 seconds
timeout /t 3 /nobreak >nul

REM Start Map001 Server directly (with channel to keep it alive)
echo [2/3] Starting Map001_new Server...
start "NightBlade - Map001_new" cmd /k "D:\MMO\NightBlade.exe -batchmode -nographics -startMapServer -mapName Map001_new -channelId 1 -centralAddress 127.0.0.1 -centralPort 6010 -publicAddress 127.0.0.1 -mapPort 8000"

REM Wait 2 seconds
timeout /t 2 /nobreak >nul

REM Start Map002 Server directly (with channel to keep it alive)
echo [3/3] Starting Map002_new Server...
start "NightBlade - Map002_new" cmd /k "D:\MMO\NightBlade.exe -batchmode -nographics -startMapServer -mapName Map002_new -channelId 1 -centralAddress 127.0.0.1 -centralPort 6010 -publicAddress 127.0.0.1 -mapPort 8001"

REM Start Dungeon001 Server directly (with channel to keep it alive)
echo [3/3] Starting Dungeon001 Server...
start "NightBlade - Dungeon001" cmd /k "D:\MMO\NightBlade.exe -batchmode -nographics -startMapServer -mapName Dungeon001 -channelId 1 -centralAddress 127.0.0.1 -centralPort 6010 -publicAddress 127.0.0.1 -mapPort 8002"


echo.
echo ========================================
echo   All servers started!
echo ========================================
echo.
echo You now have:
echo - Central + Database Server (window 1)
echo - Map001_new Server on port 8000 (window 2)
echo - Map002_new Server on port 8001 (window 3)
echo - Dungeon001 Server on port 8001 (window 4)
echo Close windows or use KILL_SERVERS.bat to stop.
echo.
pause