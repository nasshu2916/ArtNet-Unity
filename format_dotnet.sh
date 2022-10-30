#!/bin/bash -eu

cd `dirname $0`

if ! which dotnet-format &>/dev/null; then
  echo "error: dotnet-format is not installed" >&2
  exit -1
fi

dotnet-format --folder -v diagnostic "Assets/ArtNet" 
