using Splat;
using SS14.Launcher.Utility;

namespace SS14.Launcher.Localization;

/// <summary>
/// Shortcut class for convenience (wraps LocalizationManager)
/// /// </summary>
public static class Loc
{
    private static LocalizationManager localizationManager => Locator.Current.GetRequiredService<LocalizationManager>();

    public static string GetString(string sourceString)
    {
        return localizationManager.GetString(sourceString);
    }

    public static string GetParticularString(string context, string sourceString)
    {
        return localizationManager.GetParticularString(context, sourceString);
    }

    public static string GetParticularString(string context, string sourceString, params object[] args)
    {
        return localizationManager.GetParticularString(context, sourceString, args);
    }

    public static string GetParticularStringWithFallback(string context, string sourceString)
    {
        return localizationManager.GetParticularStringWithFallback(context, sourceString);
    }
}
