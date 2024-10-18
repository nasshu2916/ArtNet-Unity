#!/bin/bash -e

# check dotnet command
if ! which dotnet > /dev/null; then
  echo "error: dotnet command not found"
  exit 1
fi

# check dotnet-format
if ! dotnet tool list | grep -q dotnet-format; then
  echo "error: dotnet-format not found. Please run the 'dotnet tool restore' command to install dotnet-format"
  exit 1
fi

dotnet format whitespace Assets/ArtNet --folder
