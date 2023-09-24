#!/bin/bash

# This script updates the translation template file.

rm -rf temp
mkdir temp

# Scan program .cs sources and place results into temporary file.
# This does NOT include .xaml
echo '' > temp/program.pot
find ../SS14.Launcher -type f -iname "*.cs" | xgettext -j -f - --from-code=UTF-8 -o temp/program.pot

# Scan xaml sources and place results into temporary file.
dotnet run xamlpot "temp/xaml.pot" "../SS14.Launcher/"

# Merge two temporary files to make one combined file
xgettext -o temp/merged.pot --package-name="Space Station Multiverse Launcher" temp/program.pot temp/xaml.pot

# Fix file header
echo '# Space Station Multiverse Translation' > temp/final.pot
echo '# Released under MIT License' >> temp/final.pot
tail -n +5 temp/merged.pot >> temp/final.pot

# Move into correct folder
mv temp/final.pot ../SS14.Launcher/Assets/locale/en_US/LC_MESSAGES/Launcher.pot

# Clean temp
rm -rf temp

