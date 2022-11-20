@echo off

REM Vars
set "SLNDIR=%~dp0"

REM Restore + Build
dotnet build "%SLNDIR%\Shore" --framework net7.0 --nologo || exit /b
dotnet build "%SLNDIR%\Shore.Tests" --framework net7.0  --nologo || exit /b

REM Test
dotnet test "%SLNDIR%\Shore.Tests" --framework net7.0 --nologo --no-build
