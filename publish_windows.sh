#!/bin/bash

cd "$(dirname "$0")"

./download_net_runtime.py windows

# Clear out previous build.
rm -r **/bin bin/publish/Windows
rm SSMV.Launcher_Windows.zip

dotnet publish SS14.Launcher/SS14.Launcher.csproj /p:FullRelease=True -c Release --no-self-contained -r win-x64 /nologo /p:RobustILLink=true
dotnet publish SS14.Loader/SS14.Loader.csproj -c Release --no-self-contained -r win-x64 /nologo

dotnet build SS14.Launcher.Bootstrap/SS14.Launcher.Bootstrap.csproj -r win-x64 -c Release /nologo
dotnet publish SS14.Launcher.Bootstrap/SS14.Launcher.Bootstrap.csproj -r win-x64 -c Release /nologo

./exe_set_subsystem.py "SS14.Launcher/bin/Release/net9.0/win-x64/publish/SSMV.Launcher.exe" 2
./exe_set_subsystem.py "SS14.Loader/bin/Release/net9.0/win-x64/publish/SS14.Loader.exe" 2

# Create intermediate directories.
mkdir -p bin/publish/Windows/bin
mkdir -p bin/publish/Windows/bin/loader
mkdir -p bin/publish/Windows/dotnet

cp -r Dependencies/dotnet/windows/* bin/publish/Windows/dotnet
#cp "SS14.Launcher.Bootstrap/bin/Release/net9.0/win-x64/publish/Space Station Multiverse Launcher.exe" bin/publish/Windows
cp "SS14.Launcher.Bootstrap/bin/Release/net9.0/win-x64/publish/SS14.Launcher.Bootstrap.exe" "bin/publish/Windows/Space Station Multiverse Launcher.exe"
cp "SS14.Launcher.Bootstrap/console.bat" bin/publish/Windows
cp SS14.Launcher/bin/Release/net9.0/win-x64/publish/* bin/publish/Windows/bin
cp SS14.Loader/bin/Release/net9.0/win-x64/publish/* bin/publish/Windows/bin/loader
cp LICENSE.txt bin/publish/Windows/

# Old Fluidsynth:
# This is only needed temporarily -- it appears upstream's native update doesn't consider old robusts
# that are on old versions.  Not a concern for recent robust, but a problem for Blep, until Blep
# upgrades.  Once this is complete, should remove this.  -- Skye
cp PublishFiles/OldSynth/windows/* bin/publish/Windows/bin/loader

pushd bin/publish/Windows
zip -r ../../../SSMV.Launcher_Windows.zip *
popd
