#!/bin/bash

cd "$(dirname "$0")"

# Compile Translations
pushd TranslationTools
./compileAllMo.sh
popd

# Create builds
./publish_linux.sh
./publish_osx.sh
./publish_windows.sh
