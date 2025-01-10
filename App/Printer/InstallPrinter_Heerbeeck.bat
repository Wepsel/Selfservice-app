@echo off
setlocal enabledelayedexpansion

REM **** Logging Instellingen ****
set "logFile=C:\ProgramData\install_log.txt"
echo Logging gestart op %date% %time% > %logFile%

REM **** Verkrijg het huidige pad van waar het script wordt uitgevoerd (de werkdirectory) ****
set "currentDir=%~dp0"

REM **** Verplaats de bestanden naar C:\tmp map ****
echo [%date% %time%] - Bestanden worden gekopieerd naar C:\tmp... >> %logFile%
xcopy "%currentDir%\CanonDriver\CNP60MA64.INF" "C:\tmp\CanonDriver\" /Y >> %logFile% 2>&1
xcopy "%currentDir%\CanonDriver\cnp60m.cat" "C:\tmp\CanonDriver\" /Y >> %logFile% 2>&1
xcopy "%currentDir%\CanonDriver\gppcl6.cab" "C:\tmp\CanonDriver\" /Y >> %logFile% 2>&1
xcopy "%currentDir%\PrinterRegFiles\LPR.reg" "C:\tmp\PrinterRegFiles\" /Y >> %logFile% 2>&1
xcopy "%currentDir%\PrinterRegFiles\printer.reg" "C:\tmp\PrinterRegFiles\" /Y >> %logFile% 2>&1
if %ERRORLEVEL% neq 0 (
    echo [%date% %time%] - Fout bij het kopiëren van de bestanden! >> %logFile%
    exit /b 1
)

REM **** Enable LPD/LPR functie ****
echo [%date% %time%] - LPD/LPR functie wordt geïnstalleerd... >> %logFile%
dism /online /NoRestart /enable-feature /featurename:Printing-Foundation-LPRPortMonitor >> %logFile% 2>&1
dism /online /NoRestart /enable-feature /featurename:Printing-Foundation-LPDPrintService >> %logFile% 2>&1
if %ERRORLEVEL% neq 0 (
    echo [%date% %time%] - Fout bij het inschakelen van LPD/LPR functie! >> %logFile%
    exit /b 1
)

REM **** Printerdriver installeren via SysNative ****
echo [%date% %time%] - Printerdriver wordt geïnstalleerd... >> %logFile%
pnputil.exe -i -a "C:\tmp\CanonDriver\CNP60MA64.INF" >> %logFile% 2>&1
if %ERRORLEVEL% neq 0 (
    echo [%date% %time%] - Fout bij het installeren van de printerdriver! >> %logFile%
    exit /b 1
)

REM **** Installeer LPR-poort via registerinstellingen ****
echo [%date% %time%] - LPR-poort wordt geïnstalleerd... >> %logFile%
regedit -s "C:\tmp\PrinterRegFiles\LPR.reg" >> %logFile% 2>&1
if %ERRORLEVEL% neq 0 (
    echo [%date% %time%] - Fout bij het installeren van de LPR-poort! >> %logFile%
    exit /b 1
)

REM **** Stop LPD Service en Spooler ****
echo [%date% %time%] - Stoppen van de printservices... >> %logFile%
net stop "LPD Service" >> %logFile% 2>&1
if %ERRORLEVEL% neq 0 (
    echo [%date% %time%] - Fout bij het stoppen van LPD Service! >> %logFile%
)

net stop spooler >> %logFile% 2>&1
if %ERRORLEVEL% neq 0 (
    echo [%date% %time%] - Fout bij het stoppen van de print spooler! >> %logFile%
    exit /b 1
)

REM **** Start LPD Service en Spooler opnieuw ****
echo [%date% %time%] - Starten van de printservices... >> %logFile%
net start "LPD Service" >> %logFile% 2>&1
if %ERRORLEVEL% neq 0 (
    echo [%date% %time%] - Fout bij het starten van LPD Service! >> %logFile%
)

REM **** Printer installeren voor Heerbeeck ****
echo [%date% %time%] - Printer wordt geïnstalleerd voor Kempenhorst... >> %logFile%
rundll32 printui.dll,PrintUIEntry /if /b "VoBo-Printer (KLEUR)" /r "HBC-PR01:Uniflow-HBC-Win" /m "Canon Generic Plus PCL6" >> %logFile% 2>&1
rundll32 printui.dll,PrintUIEntry /if /b "VoBo-Printer" /r "HBC-PR01:Uniflow-HBC-Win" /m "Canon Generic Plus PCL6" >> %logFile% 2>&1
if %ERRORLEVEL% neq 0 (
    echo [%date% %time%] - Fout bij het installeren van de printer voor Heerbeeck! >> %logFile%
    exit /b 1
)

REM **** Installeer printer gerelateerde registerinstellingen na de printerinstallatie ****
echo [%date% %time%] - Printer gerelateerde registerinstellingen worden geïnstalleerd... >> %logFile%
regedit -s "C:\tmp\PrinterRegFiles\printer.reg" >> %logFile% 2>&1
if %ERRORLEVEL% neq 255 (
    echo [%date% %time%] - Fout bij het installeren van de printer reg-bestand! >> %logFile%
    exit /b 1
)

REM **** Herstart de computer om installatie te voltooien ****
echo [%date% %time%] - Printer succesvol geinstalleerd! >> %logFile%
exit /b 0