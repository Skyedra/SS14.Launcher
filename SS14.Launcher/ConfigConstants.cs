using System;

namespace SS14.Launcher;

public static class ConfigConstants
{
    public const string CurrentLauncherVersion = "1";
    public static readonly bool DoVersionCheck = true;

    // Refresh login tokens if they're within <this much> of expiry.
    public static readonly TimeSpan TokenRefreshThreshold = TimeSpan.FromDays(15);

    // If the user leaves the launcher running for absolute ages, this is how often we'll update his login tokens.
    public static readonly TimeSpan TokenRefreshInterval = TimeSpan.FromDays(7);

    // The amount of time before a server is considered timed out for status checks.
    public static readonly TimeSpan ServerStatusTimeout = TimeSpan.FromSeconds(5);

    // Check the command queue this often.
    public static readonly TimeSpan CommandQueueCheckInterval = TimeSpan.FromSeconds(1);

    public const string LauncherCommandsNamedPipeName = "SS14.Launcher.CommandPipe";
    // Amount of time to wait before the launcher decides to ignore named pipes entirely to keep the rest of the launcher functional.
    public const int LauncherCommandsNamedPipeTimeout = 150;
    // Amount of time to wait to let a redialling client properly die
    public const int LauncherCommandsRedialWaitTimeout = 1000;
    // How long to wait on various web queries.  Note that the launcher makes two of these sequentially on startup, so probably don't want this to be too long if main servers are down
    public const int MaxWebTimeout = 2500;

    public static readonly string AuthUrl = "https://central.spacestation14.io/auth/";
    public static readonly string[] DefaultHubUrls = {
        "https://cdn.spacestationmultiverse.com/hub/",
        "https://central.spacestation14.io/hub/",
        "https://cdn.blepstation.com/hub/"
    };
    public const string DiscordUrl = "https://SpaceStationMultiverse.com/discord/";
    public const string AccountBaseUrl = "https://central.spacestation14.io/web/Identity/Account/";
    public const string AccountManagementUrl = $"{AccountBaseUrl}Manage";
    public const string AccountRegisterUrl = $"{AccountBaseUrl}Register";
    public const string AccountResendConfirmationUrl = $"{AccountBaseUrl}ResendEmailConfirmation";
    public const string WebsiteUrl = "https://SpaceStationMultiverse.com/";
    public const string DownloadUrl = "https://SpaceStationMultiverse.com/downloads/";
    public const string LauncherVersionUrl = "https://cdn.blepstation.com/launcher_version.txt";
    public const string RobustBuildsManifest = "https://cdn.blepstation.com/manifest/manifest.json";
    public const string RobustModulesManifest = "https://central.spacestation14.io/builds/robust/modules.json";

    public const string UrlOverrideAssets = "https://cdn.spacestationmultiverse.com/launcher-assets/override_assets.json";
    public const string UrlAssetsBase = "https://cdn.spacestationmultiverse.com/launcher-assets/";

    public const string FallbackUsername = "JoeGenero";

    static ConfigConstants()
    {
        var envVarAuthUrl = Environment.GetEnvironmentVariable("SS14_LAUNCHER_OVERRIDE_AUTH");
        if (!string.IsNullOrEmpty(envVarAuthUrl))
            AuthUrl = envVarAuthUrl;
    }
}
