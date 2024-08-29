using System.Collections.Generic;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using SS14.Launcher.Api;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.Utility;
using SS14.Launcher.ViewModels.IdentityTabs;
using SS14.Launcher.ViewModels.Login;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.VisualTree;
using DynamicData;
using JetBrains.Annotations;
using SS14.Launcher.Localization;
using SS14.Launcher.Views;

namespace SS14.Launcher.ViewModels;

public class MainWindowIdentityViewModel : ViewModelBase
{
    private readonly DataManager _cfg;
    private readonly LoginManager _loginManager;

    // Identity Tabs
    public InformationTabViewModel InformationTab { get; }
    public KeyNewTabViewModel KeyNewTab { get; }
    public KeyImportTabViewModel KeyImportTab { get; }
    public GuestTabViewModel GuestTab { get; }
    public LoginTabViewModel WizardsDenLoginTab { get; }

    public ObservableCollection<IdentityTabViewModel> IdentityTabs { get; set; } = new ObservableCollection<IdentityTabViewModel>();

    public LanguageDropDownViewModel LanguageDropDown { get; }

    public MainWindowIdentityViewModel()
    {
        _cfg = Locator.Current.GetRequiredService<DataManager>();
        _loginManager = Locator.Current.GetRequiredService<LoginManager>();

        // Identity Tabs

        InformationTab = new InformationTabViewModel();
        KeyNewTab = new KeyNewTabViewModel();
        KeyImportTab = new KeyImportTabViewModel();
        GuestTab = new GuestTabViewModel();
        WizardsDenLoginTab = new LoginTabViewModel();

        IdentityTabs.Add(InformationTab);
        IdentityTabs.Add(KeyNewTab);
        IdentityTabs.Add(KeyImportTab);
        IdentityTabs.Add(GuestTab);
        IdentityTabs.Add(WizardsDenLoginTab);

        LanguageDropDown = new LanguageDropDownViewModel();

        _loginManager.Logins.Connect()
        .DistinctUntilChanged()
        .ObserveOn(RxApp.MainThreadScheduler)
        .Subscribe(
            _ => // Check the data after list modification actions are completed
        {
            if (!_cfg.MultiAccountsPerProvider) // Respect multi-account setting
            {
                // Replace tabs as needed
                ReplaceOrUnreplaceTabInListBasedOnAccountExisting(KeyNewTab, typeof(LoginInfoKey));
                ReplaceOrUnreplaceTabInListBasedOnAccountExisting(WizardsDenLoginTab, typeof(LoginInfoAccount)); // TODO: Properly support one account PER provider, instead of globally
            }
        });
    }

    private void ReplaceOrUnreplaceTabInListBasedOnAccountExisting(IdentityTabViewModel identityTabViewModel, Type accountType)
    {
        if (_loginManager.HasAccountOfType(accountType))
        {
            // Already has it, so remove from list if it is in there
            for (int i=0; i<IdentityTabs.Count; i++)
            {
                if (IdentityTabs[i] == identityTabViewModel)
                {
                    // IdentityTabs[i] = new AlreadyMadeTabViewModel(IdentityTabs[i].Name);
                    // (^ This syntax won't update UI)
                    IdentityTabs.Replace(IdentityTabs[i],
                        new AlreadyMadeTabViewModel(IdentityTabs[i].Name, IdentityTabs[i]));
                    return;
                }
            }
        } else {
            // Doesn't already have, so revert the replacement if necessary
            for (int i=0; i<IdentityTabs.Count; i++)
            {
                if (IdentityTabs[i] is AlreadyMadeTabViewModel alreadyMadeTabViewModel &&
                    alreadyMadeTabViewModel.ReplacementFor == identityTabViewModel)
                {
                    //IdentityTabs[i] = identityTabViewModel;
                    // (^ This syntax won't update UI)
                    IdentityTabs.Replace(IdentityTabs[i], identityTabViewModel);
                    return;
                }
            }
        }
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

    private int _selectedIndex;

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            var previous = IdentityTabs[_selectedIndex];
            previous.IsSelected = false;

            this.RaiseAndSetIfChanged(ref _selectedIndex, value);

            RunSelectedOnTab();
        }
    }

    private void RunSelectedOnTab()
    {
        var tab = IdentityTabs[_selectedIndex];
        tab.IsSelected = true;
        tab.Selected();
    }
}
