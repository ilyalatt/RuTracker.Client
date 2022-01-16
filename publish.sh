#!/bin/bash
set -e

cd src/RuTracker.Client
rm -rf bin/Release
dotnet pack -c Release
dotnet nuget push \
  bin/Release/*.nupkg \
  --source https://api.nuget.org/v3/index.json \
  --api-key $ILYALATT_NUGET_API_KEY
