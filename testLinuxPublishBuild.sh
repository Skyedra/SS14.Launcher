#!/bin/bash
# Simple script for doing a publish and running it (useful for my testing)
# (this is a bit inefficient, but tests exactly what someone would download)
./publish_linux.sh
rm -rf ../ssmvTestPublish
mkdir ../ssmvTestPublish
cp SSMV.Launcher_Linux.zip ../ssmvTestPublish/
cd ../ssmvTestPublish
unzip SSMV.Launcher_Linux.zip
./SSMV.Launcher
