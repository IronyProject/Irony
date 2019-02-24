SET pver=1.0.1
Echo Version: "%pver%"
del /q Nupkg\*.*
:: Need to delete some MSBuild-generated temp files (with .cs extension)
del /q /s ..\TemporaryGeneratedFile_*.cs
nuget.exe pack PackageSpecs\Irony.nuspec -Symbols -version %pver% -outputdirectory Nupkg
nuget.exe pack PackageSpecs\Irony.Interpreter.nuspec -Symbols -version %pver% -outputdirectory Nupkg

if "%1"=="/nopause" goto end
pause
:end