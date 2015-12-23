@echo off
REM we need nuget to install tools locally
if not exist build\tools\nuget\nuget.exe (
    ECHO Nuget not found.. Downloading..
	mkdir build\tools\nuget
    PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& 'build\download-nuget.ps1'"
)

REM we need FAKE to process our build scripts
if not exist build\tools\FAKE\tools\Fake.exe (
    ECHO FAKE not found.. Installing..
    "build\tools\nuget\nuget.exe" "install" "FAKE" "-OutputDirectory" "build\tools" "-ExcludeVersion"
)


SET TARGET="Build"
SET VERSION=

IF NOT [%1]==[] (set TARGET="%1")

IF NOT [%2]==[] (set VERSION="%2")

shift
shift

"build\tools\FAKE\tools\Fake.exe" "build\\scripts\\build.fsx" "target=%TARGET%" "version=%VERSION%"
