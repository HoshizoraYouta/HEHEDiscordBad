@echo off
REM Discord-like Application Setup Script for Windows

echo.
echo Setting up Discord-like Application...
echo.

REM Check if .NET 8 is installed
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo .NET 8 SDK is not installed. Please install it from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo .NET SDK found: %DOTNET_VERSION%
echo.

REM Check PostgreSQL
where psql >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    echo PostgreSQL found
) else (
    echo PostgreSQL not found. Please ensure PostgreSQL is installed and running.
)

echo.
echo Restoring packages...
dotnet restore

echo.
echo Building solution...
dotnet build

echo.
echo Setting up database...
echo Please ensure PostgreSQL is running and the database 'discordapp' exists.
echo To create the database, run: createdb discordapp
echo.

pause

cd src\Backend\DiscordApp.Infrastructure
dotnet ef database update --startup-project ..\DiscordApp.API\DiscordApp.API.csproj
cd ..\..\..

echo.
echo Setup complete!
echo.
echo To run the application:
echo.
echo Backend (Command Prompt 1):
echo   cd src\Backend\DiscordApp.API ^&^& dotnet run
echo.
echo Frontend (Command Prompt 2):
echo   cd src\Frontend\DiscordApp.Client ^&^& dotnet run
echo.
echo Then open the frontend URL in your browser (typically http://localhost:5173)
echo.
pause
