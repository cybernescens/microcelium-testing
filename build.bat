@echo off

set "wd=%cd%"
set "faked=%wd%\.fake"
set "microceliumd=%wd%\.microcelium"

if not exist "%faked%\*" (
 dotnet tool install fake-cli --tool-path %faked% --version 5.*
)

if not exist "%microceliumd%\*" (
 dotnet tool install microcelium-fake --tool-path %microceliumd% --version 1.*
)

@echo on
"%microceliumd%\microcelium-fake.exe" 
"%faked%\fake.exe" run build.fsx %*