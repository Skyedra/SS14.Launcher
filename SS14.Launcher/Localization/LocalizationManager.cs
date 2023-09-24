using System;
using System.Globalization;
using NGettext;

namespace SS14.Launcher.Localization;

/// <summary>
/// Manages localization for the launcher, providing functionality for setting current language and looking up
/// translated strings from GetText catalogs.
/// </summary>
public class LocalizationManager
{
    private Catalog? activeCatalog;

    public LocalizationManager()
    {
        LoadTestCulture();
    }

    private void LoadTestCulture()
    {
        var sergalTextCultureInfo = new CultureInfo("sergal");
        LoadCulture(sergalTextCultureInfo);
    }

    public void LoadCulture(CultureInfo culture)
    {
        activeCatalog = new Catalog("Launcher", "./Assets/locale", culture);
    }

    public string GetString(string sourceString)
    {
        return activeCatalog.GetString(sourceString);
    }

    public string GetParticularString(string context, string sourceString)
    {
        return activeCatalog.GetParticularString(context, sourceString);
    }

    /// <summary>
    /// This custom function allows to attempt looking up a context-specific string, and if it fails, to fallback to
    /// a non-context generic string.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="sourceString"></param>
    /// <returns></returns>
    public string GetParticularStringWithFallback(string context, string sourceString)
    {
        if (activeCatalog == null)
            return sourceString;

        if (context != null)
        {
            // Try to get string with context, and if not defined, fallback to no context version.
            // NGetText's implementation would fall through back to default non-translated language, so we do some
            // manual peeking here.

            if (activeCatalog.IsTranslationExist(context + Catalog.CONTEXT_GLUE + sourceString))
                return activeCatalog.GetParticularString(context, sourceString);
        }

        return activeCatalog.GetString(sourceString);
    }
}
