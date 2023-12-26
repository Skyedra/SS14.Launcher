using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Avalonia;
using Avalonia.Platform;
using DynamicData;
using NGettext;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
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

    /// <summary>
    /// Locale code to interpret as not having any translation loaded (ie, source language.)
    /// </summary>
    private const string DEFAULT_UNTRANSLATED_LOCALE = "en_US";

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
            var localeCulture = Language.FromLocale(localePreference);
            bool result = LoadLanguage(localeCulture);
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
        Log.Information($"Inferring user's language from system, which is set to UI: {Thread.CurrentThread.CurrentUICulture.DisplayName} and Thread: {Thread.CurrentThread.CurrentCulture.DisplayName}.");

        bool uiMatched = LoadLanguageFromCulture(Thread.CurrentThread.CurrentUICulture);
        if (uiMatched)
            return true;

        bool threadMatched = LoadLanguageFromCulture(Thread.CurrentThread.CurrentCulture);
        if (threadMatched)
            return true;

        // Couldn't find a language combo for this user
        return false;
    }

    private bool LoadLanguageFromCulture(CultureInfo targetCulture)
    {
        // Special case for developer language
        if (targetCulture.TwoLetterISOLanguageName == "en" || targetCulture.TwoLetterISOLanguageName == "iv")
        {
            return LoadLanguage(null);
        }

        var potentialLanguages = GetAvailableLanguages();

        // First search for exact culture match (ie: ru_RU)
        foreach (var language in potentialLanguages)
        {
            if (language.Value != null &&
                language.Value.cultureInfo != null &&
                language.Value.cultureInfo.Name == targetCulture.Name) // this compares region code also
            {
                if (LoadLanguage(language.Value))
                    return true;
            }
        }

        // No exact match, but is there a language match?
        foreach (var language in potentialLanguages)
        {
            if (language.Value != null &&
                language.Value.cultureInfo != null &&
                language.Value.cultureInfo.TwoLetterISOLanguageName == targetCulture.TwoLetterISOLanguageName) // this compares only language code
            {
                if (LoadLanguage(language.Value))
                    return true;
            }
        }

        return false;
    }

    private void LoadTestCulture()
    {
        var sergalLanguage = new Language("sergal", "Sergal");
        LoadLanguage(sergalLanguage);
    }

    /// <summary>
    /// Loads culture and updates stored preferences to that culture.
    /// </summary>
    /// <param name="culture"></param>
    /// <returns></returns>
    public bool SetLanguage(Language language)
    {
        bool result = LoadLanguage(language);
        if (result)
        {
            if (language != null)
                dataManager.Locale = language.translationFileName;
            else
                dataManager.Locale = DEFAULT_UNTRANSLATED_LOCALE;

            // Displays message which forces user to restart on language change.
            // Just a quick, temporary workaround until proper reloading of all strings is properly implemented.
            RequiresRestart = true;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Loads target culture.
    /// </summary>
    /// <param name="language">Language to load -- can be null to use developer/english US language.</param>
    /// <param name="culture">System Culture to load -- can be null to use default culture.</param>
    /// <returns>True is culture was changed successfully (even if changed to no culture successfully).  False if
    /// failed to load culture.</returns>
    private bool LoadLanguage(Language? language)
    {
        if (language == null || language.translationFileName == DEFAULT_UNTRANSLATED_LOCALE)
        {
            // Intentionally going back to non-translated mode
            activeCatalog = null;
            Log.Information("Disabled translations / using default English (US) locale.");

            //if (OnTranslationChanged != null)
            //    OnTranslationChanged();

            return true;
        }

        var possibleUris = new List<Uri>() {
            new Uri($"avares://SSMV.Launcher/Assets/locale/" + language.translationFileName + "/LC_MESSAGES/Launcher.mo")
        };

        CultureInfo? cultureInfo = language.cultureInfo;

        // If there's a system cultureinfo, then there's more information on possible places to check
        if (cultureInfo != null)
        {
            // This pathing logic mirrors ngettext FindTranslationFile
            possibleUris.Add(new Uri($"avares://SSMV.Launcher/Assets/locale/" + cultureInfo.Name.Replace('-', '_') + "/LC_MESSAGES/Launcher.mo"));
            possibleUris.Add(new Uri($"avares://SSMV.Launcher/Assets/locale/" + cultureInfo.Name + "/LC_MESSAGES/Launcher.mo"));
            possibleUris.Add(new Uri($"avares://SSMV.Launcher/Assets/locale/" + cultureInfo.TwoLetterISOLanguageName + "/LC_MESSAGES/Launcher.mo"));
        };

        foreach (var possibleFileUri in possibleUris)
        {
            if (AssetLoader.Exists(possibleFileUri))
            {
                var stream = AssetLoader.Open(possibleFileUri);
                if (cultureInfo != null)
                    activeCatalog = new Catalog(stream, cultureInfo);
                else
                    activeCatalog = new Catalog(stream);

                if (activeCatalog != null)
                {
                    Log.Information("Loaded translation catalog for " + language.displayName);

                    // if (OnTranslationChanged != null)
                    //     OnTranslationChanged();

                    return true;
                }
                else
                    Log.Warning("Problem loading translation catalog at " + possibleFileUri.ToString());
            }
        }

        Log.Warning("Could not find localization .po for culture: " + language.displayName);
        return false;
    }

    public string GetString(string sourceString)
    {
        if (activeCatalog == null)
            return sourceString;

        return activeCatalog.GetString(sourceString);
    }

    public string GetString(string sourceString, params object[] args)
    {
        if (activeCatalog == null)
            return String.Format(sourceString, args);

        return activeCatalog.GetString(sourceString, args);
    }

    public string GetParticularString(string context, string sourceString)
    {
        if (activeCatalog == null)
            return sourceString;

        return activeCatalog.GetParticularString(context, sourceString);
    }

    public string GetParticularString(string context, string sourceString, params object[] args)
    {
        if (activeCatalog == null)
            return String.Format(sourceString, args);

        return activeCatalog.GetParticularString(context, sourceString, args);
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
            // TODO - replace with some kind of lookup or something, since this doesn't work for test languages
            // (it shows Invariant culture)
            currentLanguageDisplayString = activeCatalog.CultureInfo.DisplayName;
            return;
        }

        currentLanguageDisplayString = "English"; // default / untranslated
    }

    public Dictionary<string, Language> GetAvailableLanguages()
    {
        // TODO: something to scan through and return available languages, maybe manage languages centrally instead
        // of recreating them in various places.

        var french = Language.FromLocale("fr_FR");
        var italian = Language.FromLocale("it");
        var german = Language.FromLocale("de");
        var spanishSpain = Language.FromLocale("es");
        var spanishMexico = Language.FromLocale("es_MX");
        var ptBr = Language.FromLocale("pt_BR");
        var zhHans = Language.FromLocale("zh_Hans");
        var ruRu = Language.FromLocale("ru_RU");
        var sergal = new Language("sergal", "Sergal");
        //var ruUa = Language.FromLocale("ru_UA");
        var ukranian = Language.FromLocale("uk");

        return new Dictionary<string, Language> {
            {"English (US)", null},
            {ruRu.displayName, ruRu},
            //{ruUa.displayName, ruUa},
            {ukranian.displayName, ukranian},
            {french.displayName, french},
            {italian.displayName, italian},
            {german.displayName, german},
            {spanishMexico.displayName, spanishMexico},
            //{"Spanish (Spain)", spanishSpain},
            {ptBr.displayName, ptBr},
            {zhHans.displayName, zhHans},
            {"Sergal", new Language("sergal", "Sergal")}
        };
    }

    public bool RequiresRestart
    {
        get { return _requiresRestart; }

        private set
        {
            this.RaiseAndSetIfChanged(ref _requiresRestart, value);
            this.RaisePropertyChanged(nameof(RequiresRestart));
        }
    }
    private bool _requiresRestart = false;
}
