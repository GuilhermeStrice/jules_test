#!/usr/bin/env bash
set -e

echo "Cleaning all projects..."
dotnet clean SAFT.Lib/SAFT.Lib.csproj
dotnet clean SAFT.Validation/SAFT.Validation.csproj
dotnet clean SAFT.Validation.Tests/SAFT.Validation.Tests.csproj

echo "Building all projects..."
dotnet build SAFT.Lib/SAFT.Lib.csproj
dotnet build SAFT.Validation/SAFT.Validation.csproj
dotnet build SAFT.Validation.Tests/SAFT.Validation.Tests.csproj

echo "Running tests..."
dotnet test SAFT.Validation.Tests/SAFT.Validation.Tests.csproj --no-build --logger "console;verbosity=normal" 