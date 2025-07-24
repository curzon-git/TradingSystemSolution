#!/bin/bash

echo "=== C# Trading System Solution Build Script ==="
echo

echo "Restoring NuGet packages..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "ERROR: Package restore failed"
    exit 1
fi

echo
echo "Building solution..."
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    echo "ERROR: Build failed"
    exit 1
fi

echo
echo "=== Build completed successfully! ==="
echo
echo "To run the trading interface:"
echo "  cd TradingWebInterface"
echo "  dotnet run"
echo
echo "To test the APIs:"
echo "  cd ClientExample"
echo "  dotnet run"
echo

