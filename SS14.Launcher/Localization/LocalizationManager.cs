using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Platform;
using DynamicData;
using NGettext;
using ReactiveUI;
using Serilog;
using Splat;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Utility;

namespace SS14.Launcher.Localization;

/// <summary>
/// Manages localization for the launcher, providing functionality for setting current language and looking up
/// translated strings from GetText catalogs.
/// </summary>
public class LocalizationManager : ReactiveObject
{
    private readonly DataManager dataManager;

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
        dataManager = Locator.Current.GetRequiredService<DataManager>();
    }

    /// <summary>
    /// Infer the best culture to use based on either the saved locale setting, or if none available, then the default
    /// one based on system settings.  Intended to be run on start up.
    /// </summary>
    public void LoadInferred()
    {
        string? localePreference = dataManager.Locale;

        if (!String.IsNullOrEmpty(localePreference))
        {
            var localeCulture = new CultureInfo(localePreference);
            bool result = LoadCulture(localeCulture);
            if (result)
                return;
            // If load failed, fall back and use system default culture
        }

        // Try to use system default culture
        LoadSystemDefault();
    }

    /// <summary>
    /// Load a default culture based on system settings
    /// </summary>
    private bool LoadSystemDefault()
    {
        // TODO - pull from OS or Steam
        return LoadCulture(null);
    }

    private void LoadTestCulture()
    {
        var sergalTextCultureInfo = new CultureInfo("sergal");
        LoadCulture(sergalTextCultureInfo);
    }

    /// <summary>
    /// Loads culture and updates stored preferences to that culture.
    /// </summary>
    /// <param name="culture"></param>
    /// <returns></returns>
    public bool SetCulture(CultureInfo culture)
    {
        bool result = LoadCulture(culture);
        if (result)
        {
            if (culture != null)
                dataManager.Locale = culture.Name;
            else
                dataManager.Locale = null;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Loads target culture.
    /// </summary>
    /// <param name="culture">Culture to load -- can be null to use developer/english US culture.</param>
    /// <returns>True is culture was changed successfully (even if changed to no culture successfully).  False if
    /// failed to load culture.</returns>
    private bool LoadCulture(CultureInfo culture)
    {
        if (culture == null)
        {
            // Intentionally going back to non-translated mode
            activeCatalog = null;
            Log.Information("Disabled translations / using default English (US) locale.");

            //if (OnTranslationChanged != null)
            //    OnTranslationChanged();

            return true;
        }

        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

        if (assets == null)
        {
            Log.Warning("Unable to find asset loader, no localization will be done.");
            return false;
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

                    if (OnTranslationChanged != null)
                        OnTranslationChanged();

                    return true;
                }
                else
                    Log.Warning("Problem loading translation catalog at " + possibleFileUri.ToString());
            }
        }

        Log.Warning("Could not find localization .po for culture: " + culture.Name);
        return false;
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

    //public Action OnTranslationChanged;
}
