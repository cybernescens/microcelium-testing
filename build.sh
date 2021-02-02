#!/bin/bash

wd=`pwd`
faked=$wd/.fake
microceliumd=$wd/.microcelium

if [ ! -d $microceliumd ] || [ -z "$(ls -A $microceliumd)" ]
then
    dotnet nuget list source | grep -io -P "(?<=\d\.)\s+.+(?=\s)" | while read -r line; do
        dotnet nuget remove source "$line"
    done

    dotnet nuget add source "https://pkgs.dev.azure.com/datadx/a3336200-6a09-4f84-bc50-005bbda90364/_packaging/DataDxNuget%40Local/nuget/v3/index.json" -n "DataDxNuget@Local"

    dotnet tool install microcelium-fake --tool-path $microceliumd --version 1.* --interactive
fi

if [ ! -d $faked ] || [ -z "$(ls -A $faked)" ]
then
    dotnet tool install fake-cli --tool-path $faked --version 5.*
fi


$microceliumd/microcelium-fake -q
$faked/fake run build.fsx "$@"
