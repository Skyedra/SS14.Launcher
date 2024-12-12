# Space Station Multiverse Launcher

Community-driven launcher for Space Station 14 forks.

[![](https://dcbadge.vercel.app/api/server/x88ymx6vBx?compact=true&style=plastic)](https://discord.gg/x88ymx6vBx) <- Discord if you would like to discuss alternate infrastructure projects or fork development :)

## Download
[Download binaries for Windows, Mac, and Linux](https://blepstation.com/download/)

[Download on Steam](https://store.steampowered.com/app/2585480/Space_Station_Multiverse/)

## Features
Differences versus current upstream version:

 * **Key Auth** - New, decentralized publickey/privatekey authentication.
 * **Translation** - Translated into multiple languages ([contribute translation](https://spacestationmultiverse.com/contribute-translation/)).
 * **Guest mode** - No auth server / registration required (for servers that support it).
 * **Offline support** - Ability to view & connect to servers as guest, even when WizDen auth server is down.
 * **Engine build mirror** - Engine downloads in this build are routed through a global CDN service, making them faster, more scalable, and less likely to have downtime.  These builds are populated by a simple script on BlepStation.com, which mirrors recent engine builds.  (This means you should still be able to connect to servers, even if you do not have engines already downloaded and the central server is down).
 * **Timeouts** - Set to faster values so you don't sit waiting for a full minute (!) during downtime.
 * **Multihub** - Manage multiple hubs, adding more reliability.  (Borrowed from [Visne's branch](https://github.com/Visne/SS14.Launcher/tree/multihub).)
 * **Manage multiple identities** - By default it is set up to allow you to manage multiple identities.
 * **Age Gate** - Enter age before you can join 18+ servers (only stored locally, not shared).
 * **Multiverse Engine** - Supports servers that run our [Multiverse engine fork](https://github.com/Space-Station-Multiverse/RobustToolbox).
 
## Limitations

 * To connect to a server without an account, the server must be configured for auth to be optional.  (At some point, I may try to figure out multi-auth).  This should work on blep and a few others.
 * There are a number of bugs yet and imperfect work arounds to make things functional.
 * Modules are not mirrored yet, so if you don't already have modules downloaded, try connecting to a server that requires WebView and WizDen's Centcomm is down, connection won't work.  I don't think most servers use WebView though.

 The launcher uses its own data path, so it should not interfere with the official launcher if you have it installed.

## Screenshots of Identity Setup

![image](https://github.com/Skyedra/SS14.Launcher/assets/22365940/bc6a9c80-278d-4e2b-b2af-450645a3c0b4)

![image](https://github.com/user-attachments/assets/ad5aa7d5-9562-40a3-b825-53090288f66c)

![image](https://github.com/Skyedra/SS14.Launcher/assets/22365940/abebd5ee-1898-4d44-b2f5-7fdaa6f17409)

## Demo of Offline Mode

This video demonstrates offline mode working even when WizDen was offline.  (This video is a bit older, shows UI before the new identity setup layout was done)

https://github.com/Skyedra/SS14.Launcher/assets/22365940/f182fd58-ccb8-4387-9f7a-198f957fc71d

## More Screenshots

![image](https://github.com/Skyedra/SS14.Launcher/assets/22365940/786a1765-32ab-42f5-9358-316a7ad4498a)


---

# (Regular launcher info follows)

This is the launcher you should be using to connect to SS14 servers. Server browser, content downloads, account management. It's got it all!

# Development

Useful environment variables for development:
* `SS14_LAUNCHER_APPDATA_NAME=launcherTest` to change the user data directories the launcher stores its data in. This can be useful to avoid breaking your "normal" SS14 launcher data while developing something.
* `SS14_LAUNCHER_OVERRIDE_AUTH=https://.../` to change the auth API URL to test against a local dev version of the API.
