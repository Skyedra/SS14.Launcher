# Experimental Offline SS14 Launcher

This is a customized, experimental, non-official version of the [Space Station 14](https://spacestation14.io/) launcher which *should* be more resilient against downtime.

The purpose of this is so you can still play the game, even when Centcomm is down.

## Features
Differences versus current upstream version:

 * **No auth required / Offline mode** - ability to view & connect to servers as guest, even when auth server is down.
 * **Engine build mirror** - engine downloads in this build are routed through a global CDN service.  These builds are populated by a simple script on BlepStation.com, which mirrors recent engine builds.  (This means you should still be able to connect to servers, even if you do not have engines already downloaded and the central server is down).
 * **Timeouts** - Set to faster values so you don't sit waiting for a full minute (!) during downtime.
 * **Multihub** - Manage multiple hubs, in case main hub is down.  Borrowed from [Visne's branch](https://github.com/Visne/SS14.Launcher/tree/multihub).  (Note - no alternate hubs are included by default, though I might try to build a mirror of upstream's hub later.)

## Limitations

 * To connect to a server without an account, the server must be configured for auth to be optional.  (At some point, I may try to figure out multi-auth)
 * If main hub is down, you will need to add your servers to your favorites list or direct connect.
 * Modules are not mirrored, so if you don't already have modules downloaded, try connecting to a server that requires WebView and Centcomm is down, connection won't work.  I don't think most servers use WebView though.

## Demo

https://github.com/Skyedra/SS14.Launcher/assets/22365940/f182fd58-ccb8-4387-9f7a-198f957fc71d

(Upstream are welcome to take this code if interested, but there are still some bugs and possible improvements as of this writing)

---

# (Regular launcher info follows)

This is the launcher you should be using to connect to SS14 servers. Server browser, content downloads, account management. It's got it all!

# Development

Useful environment variables for development:
* `SS14_LAUNCHER_APPDATA_NAME=launcherTest` to change the user data directories the launcher stores its data in. This can be useful to avoid breaking your "normal" SS14 launcher data while developing something.
* `SS14_LAUNCHER_OVERRIDE_AUTH=https://.../` to change the auth API URL to test against a local dev version of the API.
