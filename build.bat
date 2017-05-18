@echo off

nuget restore ScrapTheWorld.sln

set KEY_NAME="HKLM\SOFTWARE\Microsoft\MSBuild\ToolsVersions\4.0"
set VALUE_NAME=MSBuildToolsPath

FOR /F "usebackq skip=2 tokens=1-3" %%A IN (`REG QUERY %KEY_NAME% /v %VALUE_NAME% 2^>nul /reg:32`) DO (
    set msbuildPath=%%Cmsbuild.exe
)

if defined msbuildPath (
    @echo %msbuildPath%
    %msbuildPath% ScrapTheWorld.sln /p:Configuration=Release /p:Platform=x86 /p:BuildInParallel=true /p:VisualStudioVersion=14.0 /tv:14.0
) else (
    @echo %KEY_NAME%\%VALUE_NAME% not found.
)

pause