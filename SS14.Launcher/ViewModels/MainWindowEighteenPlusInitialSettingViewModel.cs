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

public class MainWindowEighteenPlusInitialSettingViewModel : ViewModelBase
{
    private readonly DataManager _cfg;

    public LanguageDropDownViewModel LanguageDropDown { get; }
    public AgeManager ageManager;
    MainWindowViewModel mainWindowViewModel;
    [Reactive] public DateTimeOffset? EnteredBirthDate { get; set; }

    public MainWindowEighteenPlusInitialSettingViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _cfg = Locator.Current.GetRequiredService<DataManager>();
        ageManager = Locator.Current.GetRequiredService<AgeManager>();

        LanguageDropDown = new LanguageDropDownViewModel();
        this.mainWindowViewModel = mainWindowViewModel;
    }

    public string Version => $"v{LauncherVersion.Version}";

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

    public void YesButtonPressed()
    {
        // Clear both the "yes" and "no" filters to reset it to show all servers.
        mainWindowViewModel.ServersTab.Filters.SetFilter(
            new ServerFilter(ServerFilterCategory.EighteenPlus, ServerFilter.DataTrue), false);
        mainWindowViewModel.ServersTab.Filters.SetFilter(
            new ServerFilter(ServerFilterCategory.EighteenPlus, ServerFilter.DataFalse), false);

        _cfg.SetCVar(CVars.InitialEighteenPlusPreferenceSet, true);

        mainWindowViewModel.CalculateActiveMainWindow();

    }

    public void NoButtonPressed()
    {
        // Disable "yes" and enable "no" filters to hide 18+ servers.
        mainWindowViewModel.ServersTab.Filters.SetFilter(
            new ServerFilter(ServerFilterCategory.EighteenPlus, ServerFilter.DataTrue), false);
        mainWindowViewModel.ServersTab.Filters.SetFilter(
            new ServerFilter(ServerFilterCategory.EighteenPlus, ServerFilter.DataFalse), true);

        _cfg.SetCVar(CVars.InitialEighteenPlusPreferenceSet, true);

        mainWindowViewModel.CalculateActiveMainWindow();
    }
}
