#! /usr/bin/env bash

set -eu

repo_root="$(dirname $0)"

cd "$repo_root"
rm -rf integ-test/local-packages
dotnet pack -o ./integ-test/local-packages

cd "$repo_root/integ-test/src"
rm -rf packages
dotnet restore --packages ./packages --configfile ../nuget.config
dotnet build -c Release --packages ./packages --no-restore
dotnet test -c Release --no-build --no-restore
