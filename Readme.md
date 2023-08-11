# Space Station Multiverse Launcher

This is a customized, experimental, non-official version of the [Space Station 14](https://spacestation14.io/) launcher.

Basically it is a version of the official launcher with more features (and bugs).

[![](https://dcbadge.vercel.app/api/server/x88ymx6vBx?compact=true&style=plastic)](https://discord.gg/x88ymx6vBx) <- Discord if you would like to discuss alternate infrastructure projects or fork development :)

## Download
[Download binaries for Windows, Mac, and Linux](https://blepstation.com/download/).

## Features
Differences versus current upstream version:

 * **Guest mode** - No auth server / registration required, but also compatible with WizDen accounts.  (Plans for more account providers in future)
 * **Offline support** - Ability to view & connect to servers as guest, even when WizDen auth server is down.
 * **Engine build mirror** - Engine downloads in this build are routed through a global CDN service, making them faster, more scalable, and less likely to have downtime.  These builds are populated by a simple script on BlepStation.com, which mirrors recent engine builds.  (This means you should still be able to connect to servers, even if you do not have engines already downloaded and the central server is down).
 * **Timeouts** - Set to faster values so you don't sit waiting for a full minute (!) during downtime.
 * **Multihub** - Manage multiple hubs, adding more reliability.  (Borrowed from [Visne's branch](https://github.com/Visne/SS14.Launcher/tree/multihub).)
 * **Hub mirror** - Mirrors WizDen hub's primary API call so you can get a list of servers, even if WizDen is down (this is also routed through a real CDN for reliability & scalability.)
 * **Manage multiple identities** - By default it is set up to allow you to manage multiple identities.

## Limitations

 * To connect to a server without an account, the server must be configured for auth to be optional.  (At some point, I may try to figure out multi-auth).  This should work on blep and a few others.
 * There are a number of bugs yet and imperfect work arounds to make things functional.
 * Modules are not mirrored yet, so if you don't already have modules downloaded, try connecting to a server that requires WebView and WizDen's Centcomm is down, connection won't work.  I don't think most servers use WebView though.

 The launcher uses its own data path, so it should not interfere with the official launcher if you have it installed.

## Screenshots of Identity Setup

![image](https://github.com/Skyedra/SS14.Launcher/assets/22365940/206b45bc-6626-4465-8242-d49680d3d74a)

![image](https://github.com/Skyedra/SS14.Launcher/assets/22365940/2ca7b5fe-cb4e-4163-8323-b5c72e535b36)

## Demo of Offline Mode

This video demonstrates offline mode working even when WizDen was offline.  (This video is a bit older, shows UI before the new identity setup layout was done)

https://github.com/Skyedra/SS14.Launcher/assets/22365940/f182fd58-ccb8-4387-9f7a-198f957fc71d


---

# (Regular launcher info follows)

This is the launcher you should be using to connect to SS14 servers. Server browser, content downloads, account management. It's got it all!

# Development

Useful environment variables for development:
* `SS14_LAUNCHER_APPDATA_NAME=launcherTest` to change the user data directories the launcher stores its data in. This can be useful to avoid breaking your "normal" SS14 launcher data while developing something.
* `SS14_LAUNCHER_OVERRIDE_AUTH=https://.../` to change the auth API URL to test against a local dev version of the API.
