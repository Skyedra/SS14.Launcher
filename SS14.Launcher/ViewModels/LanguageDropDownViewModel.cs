using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive.Linq;
using DynamicData;
using JetBrains.Annotations;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Splat;
using SS14.Launcher.Api;
using SS14.Launcher.Localization;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.Utility;


namespace SS14.Launcher.ViewModels;

public class LanguageDropDownViewModel : ViewModelBase
{
    private readonly LocalizationManager localizationManager;

    public List<AvailableLanguageViewModel> availableLanguages { get; set; }

    public LanguageDropDownViewModel()
    {
        localizationManager = Locator.Current.GetRequiredService<LocalizationManager>();

        // Populate availableLanguages from LocalizationManager
        availableLanguages = new();
        foreach (var languageFromManager in localizationManager.GetAvailableLanguages())
        {
            var languageViewModel = new AvailableLanguageViewModel(languageFromManager.Value, languageFromManager.Key);
            availableLanguages.Add(languageViewModel);
        }

        // Update language dropdown button text when language has changed
        this.WhenAnyValue(x => x.localizationManager.currentLanguageDisplayString)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(CurrentLanguageButtonText));
            });
    }

    public string CurrentLanguageButtonText
    {
        get
        {
            return "🌎 " + localizationManager.currentLanguageDisplayString;
        }
    }

    [Reactive] public bool IsDropDownOpen { get; set; }


    public void SwitchLanguageDueToButtonPress(CultureInfo language)
    {
        IsDropDownOpen = false;

        localizationManager.SetCulture(language);
    }
}

public sealed class AvailableLanguageViewModel : ViewModelBase
{
    public CultureInfo language { get; private set; }
    public string displayText { get; private set; }

    public AvailableLanguageViewModel(CultureInfo language, string displayText)
    {
        this.language = language;
        this.displayText = displayText;
    }
}
