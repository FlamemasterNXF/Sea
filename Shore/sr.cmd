@echo off

REM Vars
set "SLNDIR=%~dp0"

REM Restore + Build
dotnet build "%SLNDIR%\sr" --nologo || exit /b

REM Run
dotnet run --project "%SLNDIR%\sr" --no-build -- %*