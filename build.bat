@echo off
echo === C# Trading System Solution Build Script ===
echo.

echo Restoring NuGet packages...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Package restore failed
    pause
    exit /b 1
)

echo.
echo Building solution...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)

echo.
echo === Build completed successfully! ===
echo.
echo To run the trading interface:
echo   cd TradingWebInterface
echo   dotnet run
echo.
echo To test the APIs:
echo   cd ClientExample  
echo   dotnet run
echo.
pause

