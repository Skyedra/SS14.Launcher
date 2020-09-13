using System;

namespace SS14.Launcher
{
    public static class ConfigConstants
    {
        public const string CurrentLauncherVersion = "5";
        public const bool DoVersionCheck = true;

        // Refresh login tokens if they're within <this much> of expiry.
        public static readonly TimeSpan TokenRefreshThreshold = TimeSpan.FromDays(15);
        // If the user leaves the launcher running for absolute ages, this is how often we'll update his login tokens.
        public static readonly TimeSpan TokenRefreshInterval = TimeSpan.FromDays(7);

        public const string HubUrl = "https://builds.spacestation14.io/hub/";
        public const string AuthUrl = "http://localhost:5000/";
        public const string DiscordUrl = "https://discord.gg/t2jac3p";
        public const string WebsiteUrl = "https://spacestation14.io";
        public const string DownloadUrl = "https://spacestation14.io/about/nightlies/";
    }
}