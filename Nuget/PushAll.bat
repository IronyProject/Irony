SET pver=1.5.2
Echo Version: "%pver%"
dir ..\packages\*.nupkg
@echo off
setlocal
:PROMPT
SET AREYOUSURE=N
SET /P AREYOUSURE=Are you sure (Y/[N])?
IF /I "%AREYOUSURE%" NEQ "Y" GOTO END

echo Publishing....
:: When we push bin package, the symbols package is pushed automatically by the nuget util
nuget push ..\packages\Irony.%pver%.nupkg -source https://api.nuget.org/v3/index.json 
nuget push ..\packages\Irony.Interpreter.%pver%.nupkg -source https://api.nuget.org/v3/index.json 
pause

:END
endlocal

