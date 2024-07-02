using System.Collections.Generic;
using ReactiveUI;
using Splat;
using SS14.Launcher.Api;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.Utility;
using SS14.Launcher.ViewModels.IdentityTabs;
using SS14.Launcher.ViewModels.Login;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using SS14.Launcher.Localization;
using SS14.Launcher.Models;

namespace SS14.Launcher.ViewModels;

public class MainWindowAgeViewModel : ViewModelBase
{
    private readonly DataManager _cfg;

    public LanguageDropDownViewModel LanguageDropDown { get; }
    public AgeManager ageManager;
    MainWindowViewModel mainWindowViewModel;
    [Reactive] public DateTimeOffset? EnteredBirthDate { get; set; }

    public MainWindowAgeViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _cfg = Locator.Current.GetRequiredService<DataManager>();
        ageManager = Locator.Current.GetRequiredService<AgeManager>();

        LanguageDropDown = new LanguageDropDownViewModel();
        this.mainWindowViewModel = mainWindowViewModel;

        this.WhenAnyValue(x => x.EnteredBirthDate)
            .Subscribe(x =>
            {
                this.RaisePropertyChanged(nameof(CalculatedAgeText));
            });
    }

    public string Version => $"v{LauncherVersion.Version}";

    public string CalculatedAgeText
    {
        get
        {
            if (EnteredBirthDate == null || EnteredBirthDate > DateTime.Now)
                return "";

            int yearsOld = ageManager.YearsOld(DateOnly.FromDateTime(EnteredBirthDate.Value.LocalDateTime));
            return Loc.GetParticularString("Age Entry Window", "You are {0} year(s) old.", yearsOld);
        }
    }

    public bool LogLauncher
    {
        // This not a clean solution, replace it with something better.
        get => _cfg.GetCVar(CVars.LogLauncher);
        set
        {
            _cfg.SetCVar(CVars.LogLauncher, value);
            _cfg.CommitConfig();
        }
    }

    public void SubmitButtonPressed()
    {
        // Submit pressed -- validate age entered & save it if so

        if (!EnteredBirthDate.HasValue)
        {
            ShowError(Loc.GetParticularString("Age Entry Window", "Please enter a valid birth date."));
            return;
        }

        // Verify not in future, or too far in past
        if (!ageManager.IsValidAge(DateOnly.FromDateTime(EnteredBirthDate.Value.LocalDateTime)))
        {
            ShowError(Loc.GetParticularString("Age Entry Window", "Please enter a valid birth date."));
            return;
        }

        ageManager.BirthDate = DateOnly.FromDateTime(EnteredBirthDate.Value.LocalDateTime);
        mainWindowViewModel.CalculateActiveMainWindow();
    }

    private void ShowError(string errorText)
    {
        mainWindowViewModel.AgeErrorText = errorText;
        mainWindowViewModel.ShowAgeError = true;
    }
}
