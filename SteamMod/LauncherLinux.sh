#!/bin/bash

# Check if ".data_moved" file exists
if [ ! -f .data_moved ]; then
    # Check if ".local/share/Space Station 14/launcher" exists
    if [ -d "$HOME/.local/share/Space Station 14/launcher" ]; then
        echo "Renaming 'launcher' folder to 'launcher-skyedra'..."
        cp "$HOME/.local/share/Space Station 14/launcher" "$HOME/.local/share/Space Station 14/launcher-skyedra"
        echo "Folder renaming complete."

        echo "Creating '.data_moved' file..."
        touch .data_moved
    else
        echo "'launcher' folder not found. Skipping renaming."
    fi
else
    echo "'.data_moved' file exists. Skipping folder renaming and file creation."
fi

# Check if ".multiverse" file exists
if [ -f .multiverse ]; then
    echo "'.multiverse' file found. Skipping script and launching SS14.Launcher."
    ./SS14.Launcher
else
    echo "Downloading SS14.Launcher_Linux.zip..."
    wget https://cdn.spacestationmultiverse.com/launcher-builds/SS14.Launcher_Linux.zip

    echo "Deleting existing files except the script and zip..."
    find . -maxdepth 1 ! -name "$(basename $0)" ! -name "SS14.Launcher_Linux.zip" -exec rm -r {} \;

    echo "Extracting SS14.Launcher_Linux.zip..."
    unzip SS14.Launcher_Linux.zip
    rm SS14.Launcher_Linux.zip
    touch .multiverse
    ./SS14.Launcher

    echo "Script execution complete."
fi
