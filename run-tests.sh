#!/usr/bin/env bash
set -e

# Clean, build, and test the entire solution

echo "Cleaning solution..."
dotnet clean SAFT.sln

echo "Building solution..."
dotnet build SAFT.sln

echo "Running tests..."
dotnet test SAFT.sln --no-build --logger "console;verbosity=normal" 