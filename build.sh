#!/bin/bash

wd=`pwd`
faked=$wd/.fake
microceliumd=$wd/.microcelium

if [ ! -d $microceliumd ] || [ -z "$(ls -A $microceliumd)" ]
then
    dotnet tool install microcelium-fake --tool-path $microceliumd --version 1.* --interactive
fi

if [ ! -d $faked ] || [ -z "$(ls -A $faked)" ]
then
    dotnet tool install fake-cli --tool-path $faked --version 5.*
fi


$microceliumd/microcelium-fake -q
$faked/fake run build.fsx "$@"
