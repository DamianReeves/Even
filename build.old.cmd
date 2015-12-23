@echo off
cd %~dp0

SETLOCAL
SET NUGET_VERSION=latest
SET CACHED_NUGET=%LocalAppData%\NuGet\nuget.%NUGET_VERSION%.exe
SET BUILDCMD_FAKEBUILD_VERSION=
SET BUILDCMD_DNX_VERSION=

IF EXIST %CACHED_NUGET% goto copynuget
echo Downloading latest version of NuGet.exe...
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://dist.nuget.org/win-x86-commandline/%NUGET_VERSION%/nuget.exe' -OutFile '%CACHED_NUGET%'"

:copynuget
IF EXIST .nuget\nuget.exe goto restore
md .nuget
copy %CACHED_NUGET% .nuget\nuget.exe > nul

:restore
IF EXIST packages\FAKE\tools goto restoreSourceLink
IF "%BUILDCMD_FAKEBUILD_VERSION%"=="" (
    .nuget\NuGet.exe install FAKE -ExcludeVersion -Source https://www.nuget.org/api/v2/ -Out src\packages  -nocache -pre
) ELSE (
    .nuget\NuGet.exe install FAKE -version %BUILDCMD_FAKEBUILD_VERSION% -ExcludeVersion -Source https://www.nuget.org/api/v2/ -Out src\packages  -nocache -pre
)

:restoreSourceLink
IF EXIST src\packages\SourceLink.FAKE\tools goto restorePackages
IF "%BUILDCMD_FAKEBUILD_VERSION%"=="" (
    .nuget\NuGet.exe install SourceLink.FAKE -ExcludeVersion -Source https://www.nuget.org/api/v2/ -Out src\packages  -nocache -pre
) ELSE (
    .nuget\NuGet.exe install SourceLink.FAKE -version %BUILDCMD_FAKEBUILD_VERSION% -ExcludeVersion -Source https://www.nuget.org/api/v2/ -Out src\packages  -nocache -pre
)

:restorePackages
REM .nuget\NuGet.exe restore src\packages.config -PackagesDirectory src\packages

:runFake
src\packages\FAKE\tools\FAKE.exe build.fsx %*