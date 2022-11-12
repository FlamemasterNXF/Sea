@echo off

REM Vars
set "SLNDIR=%~dp0"

REM Restore + Build
dotnet build "%SLNDIR%\Shore.sln" --nologo || exit /b

REM Test
dotnet test "%SLNDIR%\Shore.Tests" --nologo --no-build
