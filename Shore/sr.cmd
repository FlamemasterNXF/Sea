@echo off

REM Vars
set "SLNDIR=%~dp0"

REM Restore + Build
dotnet build "%SLNDIR%\sr" --framework net7.0 --nologo || exit /b

REM Run
dotnet run --project "%SLNDIR%\sr" --framework net7.0 --no-build -- %*