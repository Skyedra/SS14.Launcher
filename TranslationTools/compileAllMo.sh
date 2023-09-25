#!/bin/bash

# Loop through all PO files and compile them to .MO

for po in `find ../SS14.Launcher -name 'Launcher.po' -type f`
do
	echo $po:
	mo=`echo $po | sed -e 's/\.po/.mo/'`
	#echo $mo
	msgfmt --use-fuzzy "$po" -o "$mo"
done
