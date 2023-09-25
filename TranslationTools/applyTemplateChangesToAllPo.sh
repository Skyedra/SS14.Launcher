#!/bin/bash

# Loop through all PO files and update them with latest translations from template file

template="../SS14.Launcher/Assets/locale/en_US/LC_MESSAGES/Launcher.pot"

for po in `find ../SS14.Launcher -name 'Launcher.po' -type f`
do
	echo $po:
	msgmerge -U --backup=none --no-fuzzy-matching "$po" "$template"
done
