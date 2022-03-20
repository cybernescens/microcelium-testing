#!/bin/bash

wd=$(pwd)
faked=$wd/.fake
microceliumd=$wd/.microcelium

if [[ ! -d "$microceliumd" ]]
then
    dotnet tool install microcelium-fake --tool-path $microceliumd --version 1.* --interactive
fi

if [[ ! -d "$faked" ]]
then
    dotnet tool install fake-cli --tool-path $faked --version 5.*
fi

echo $microceliumd

$microceliumd/microcelium-fake -q
$faked/fake run build.fsx "$@"
