using System;
using System.Globalization;

namespace SS14.Launcher.Localization;

/// <summary>
/// Represents a user selectable language for the localization system.
/// </summary>
public class Language
{
    public CultureInfo? cultureInfo;
    public string translationFileName;
    public string displayName;

    public Language(CultureInfo cultureInfo, string translationFileName, string displayName)
    {
        this.cultureInfo = cultureInfo;
        this.translationFileName = translationFileName;
        this.displayName = displayName;
    }

    /// <summary>
    /// Create language without culture info -- needed for not-real languages like test languages, since on windows
    /// trying to init a culture info that doesn't exist in OS will throw an exception.
    /// </summary>
    /// <param name="translationFileName"></param>
    /// <param name="displayName"></param>
    public Language(string translationFileName, string displayName)
    {
        this.translationFileName = translationFileName;
        this.displayName = displayName;
    }

    /// <summary>
    /// Shortcut method that tries to look up system culture code if it exists
    /// </summary>
    /// <param name="cultureCode"></param>
    public static Language FromLocale(string cultureCode)
    {
        CultureInfo? cultureInfo = null;
        try
        {
            cultureInfo = new CultureInfo(cultureCode);
        } catch (Exception e) { }  // Perfectly normal for this to fail for test languages

        if (cultureInfo != null)
            return new Language(cultureInfo, cultureCode, cultureInfo.DisplayName);
        else
            return new Language(cultureCode, cultureCode); // this is definitely non-ideal since it doesn't look up display name -- we need to do a proper lookup for this later, though this only affects test language in practicality

    }
}
