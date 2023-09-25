using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Platform;
using DynamicData;
using NGettext;
using ReactiveUI;
using Serilog;

namespace SS14.Launcher.Localization;

/// <summary>
/// Manages localization for the launcher, providing functionality for setting current language and looking up
/// translated strings from GetText catalogs.
/// </summary>
public class LocalizationManager : ReactiveObject
{
    private Catalog? _activeCatalog;
    private Catalog? activeCatalog
    {
        get
        {
            return _activeCatalog;
        }

        set
        {
            _activeCatalog = value;

            // Trigger language name to be updated, which can update UI if needed
            UpdateCurrentLanguageDisplayString();
        }
    }

    public LocalizationManager()
    {
    }

    public void LoadDefault()
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
        if (culture == null)
        {
            // Intentionally going back to non-translated mode
            activeCatalog = null;
            Log.Information("Disabled translations / using default English (US) locale.");
            return;
        }

        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

        if (assets == null)
        {
            Log.Warning("Unable to find asset loader, no localization will be done.");
            return;
        }

        // This pathing logic mirrors ngettext FindTranslationFile
        var possibleUris = new[] {
            new Uri($"avares://SSMV.Launcher/Assets/locale/" + culture.Name.Replace('-', '_') + "/LC_MESSAGES/Launcher.mo"),
            new Uri($"avares://SSMV.Launcher/Assets/locale/" + culture.Name + "/LC_MESSAGES/Launcher.mo"),
            new Uri($"avares://SSMV.Launcher/Assets/locale/" + culture.TwoLetterISOLanguageName + "/LC_MESSAGES/Launcher.mo")
        };

        foreach (var possibleFileUri in possibleUris)
        {
            if (assets.Exists(possibleFileUri))
            {
                var stream = assets.Open(possibleFileUri);
                activeCatalog = new Catalog(stream, culture);

                if (activeCatalog != null)
                {
                    Log.Information("Loaded translation catalog for " + culture.Name);
                    return;
                }
                else
                    Log.Warning("Problem loading translation catalog at " + possibleFileUri.ToString());
            }
        }

        Log.Warning("Could not find localization .po for culture: " + culture.Name);
    }

    public string GetString(string sourceString)
    {
        if (activeCatalog == null)
            return sourceString;

        return activeCatalog.GetString(sourceString);
    }

    public string GetParticularString(string context, string sourceString)
    {
        if (activeCatalog == null)
            return sourceString;

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

    public string currentLanguageDisplayString {
        get
        {
            return _currentLanguageDisplayString;
        }

        private set
        {
            // this allows for the property to be updated on UI buttons
            this.RaiseAndSetIfChanged(ref _currentLanguageDisplayString, value);
        }
    }
    private string _currentLanguageDisplayString;

    private void UpdateCurrentLanguageDisplayString()
    {
        if (activeCatalog != null)
        {
            currentLanguageDisplayString = activeCatalog.CultureInfo.DisplayName;
            return;
        }

        currentLanguageDisplayString = "English"; // default / untranslated
    }

    public Dictionary<string, CultureInfo> GetAvailableLanguages()
    {
        // TODO: something to scan through and return available languages

        return new Dictionary<string, CultureInfo> {
            {"English (US)", null},
            {"Sergal", new CultureInfo("sergal")}
        };
    }
}
