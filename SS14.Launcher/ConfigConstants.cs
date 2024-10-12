using System;
using SS14.Launcher.Utility;

namespace SS14.Launcher;

public static class ConfigConstants
{
    public const string CurrentLauncherVersion = "2";
    public static readonly bool DoVersionCheck = true;

    // Refresh login tokens if they're within <this much> of expiry.
    public static readonly TimeSpan TokenRefreshThreshold = TimeSpan.FromDays(15);

    // If the user leaves the launcher running for absolute ages, this is how often we'll update his login tokens.
    public static readonly TimeSpan TokenRefreshInterval = TimeSpan.FromDays(7);

    // The amount of time before a server is considered timed out for status checks.
    public static readonly TimeSpan ServerStatusTimeout = TimeSpan.FromSeconds(5);

    // Check the command queue this often.
    public static readonly TimeSpan CommandQueueCheckInterval = TimeSpan.FromSeconds(1);

    public const string LauncherCommandsNamedPipeName = "SSMV.Launcher.CommandPipe";
    // Amount of time to wait before the launcher decides to ignore named pipes entirely to keep the rest of the launcher functional.
    public const int LauncherCommandsNamedPipeTimeout = 150;
    // Amount of time to wait to let a redialling client properly die
    public const int LauncherCommandsRedialWaitTimeout = 1000;
    // How long to wait on various web queries.  Note that the launcher makes two of these sequentially on startup, so probably don't want this to be too long if main servers are down
    public const int MaxWebTimeout = 10000;

    public static readonly string AuthUrl = "https://auth.spacestation14.com/";
    public static readonly string[] DefaultHubUrls = {
        "https://cdn.spacestationmultiverse.com/hub/",
        //"https://hub.spacestation14.com/",
        //"https://cdn.spacestationmultiverse.com/wizden-hub-mirror/"
    };
    public const string DiscordUrl = "https://SpaceStationMultiverse.com/discord/";
    public const string ContributeLocalizationUrl = "https://spacestationmultiverse.com/contribute-translation/";

    public const string AccountBaseUrl = "https://account.spacestation14.com/Identity/Account/";
    public const string AccountManagementUrl = $"{AccountBaseUrl}Manage";
    public const string AccountRegisterUrl = $"{AccountBaseUrl}Register";
    public const string AccountResendConfirmationUrl = $"{AccountBaseUrl}ResendEmailConfirmation";
    public const string WebsiteUrl = "https://SpaceStationMultiverse.com/";
    public const string DownloadUrl = "https://SpaceStationMultiverse.com/downloads/";
    public const string NewsFeedUrl = "https://spacestationmultiverse.com/rss";
    public const string LauncherVersionUrl = "https://cdn.blepstation.com/launcher_version.txt";
    public static readonly UrlFallbackSet RobustBuildsManifest = new ([
        "https://cdn.blepstation.com/manifest/manifest.json",
        // Can fall back to wizden servers as the data will be the same:
        "https://robust-builds.cdn.spacestation14.com/manifest.json",
        "https://robust-builds.fallback.cdn.spacestation14.com/manifest.json"
    ]);

    public static readonly UrlFallbackSet RobustModulesManifest = new([
        "https://robust-builds.cdn.spacestation14.com/modules.json",
        "https://robust-builds.fallback.cdn.spacestation14.com/modules.json"
    ]);

    public static readonly UrlFallbackSet MultiverseEngineBuildsManifest = new([
        "https://cdn.spacestationmultiverse.com/ssmv-engine-manifest"
    ]);

    // How long to keep cached copies of Robust manifests.
    // TODO: Take this from Cache-Control header responses instead.
    public static readonly TimeSpan RobustManifestCacheTime = TimeSpan.FromMinutes(15);

    public static readonly UrlFallbackSet UrlOverrideAssets = new (["https://cdn.spacestationmultiverse.com/launcher-assets/override_assets.json"]);
    public static readonly UrlFallbackSet UrlAssetsBase = new (["https://cdn.spacestationmultiverse.com/launcher-assets/"]);

    // Currently contains server-set messages.
    // In the future, planning to merge launcher version and override assets info,
    // so we can coalesce all of that into a single HTTP request at startup.
    public static readonly UrlFallbackSet UrlLauncherInfo = new (["https://cdn.spacestationmultiverse.com/launcher-assets/info.json"]);

    public const string FallbackUsername = "JoeGenero";

    static ConfigConstants()
    {
        var envVarAuthUrl = Environment.GetEnvironmentVariable("SS14_LAUNCHER_OVERRIDE_AUTH");
        if (!string.IsNullOrEmpty(envVarAuthUrl))
            AuthUrl = envVarAuthUrl;
    }
}
