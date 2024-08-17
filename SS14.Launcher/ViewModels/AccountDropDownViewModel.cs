using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.VisualTree;
using DynamicData;
using JetBrains.Annotations;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using SS14.Launcher.Api;
using SS14.Launcher.Localization;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.Utility;
using SS14.Launcher.Views;



namespace SS14.Launcher.ViewModels;

public class AccountDropDownViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainVm;
    private readonly DataManager _cfg;
    private readonly AuthApi _authApi;
    private readonly LoginManager _loginMgr;
    private readonly ReadOnlyObservableCollection<AvailableAccountViewModel> _accounts;

    public ReadOnlyObservableCollection<AvailableAccountViewModel> Accounts => _accounts;

    public Control? Control { get; set; }

    public AccountDropDownViewModel(MainWindowViewModel mainVm)
    {
        _mainVm = mainVm;
        _cfg = Locator.Current.GetRequiredService<DataManager>();
        _authApi = Locator.Current.GetRequiredService<AuthApi>();
        _loginMgr = Locator.Current.GetRequiredService<LoginManager>();

        this.WhenAnyValue(x => x._loginMgr.ActiveAccount)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(LoginText));
                this.RaisePropertyChanged(nameof(AccountSwitchText));
                this.RaisePropertyChanged(nameof(LogoutText));
                this.RaisePropertyChanged(nameof(ConfigureText));
                this.RaisePropertyChanged(nameof(AccountControlsVisible));
                this.RaisePropertyChanged(nameof(AccountSwitchVisible));
                this.RaisePropertyChanged(nameof(LogoutButtonVisible));
                this.RaisePropertyChanged(nameof(ConfigureButtonVisible));
            });

        _loginMgr.Logins.Connect().Subscribe(_ =>
        {
            this.RaisePropertyChanged(nameof(LogoutText));
            this.RaisePropertyChanged(nameof(ConfigureText));
            this.RaisePropertyChanged(nameof(AccountSwitchVisible));
            this.RaisePropertyChanged(nameof(LogoutButtonVisible));
            this.RaisePropertyChanged(nameof(ConfigureButtonVisible));
        });

        var filterObservable = this.WhenAnyValue(x => x._loginMgr.ActiveAccount)
            .Select(MakeFilter);

        _loginMgr.Logins
            .Connect()
            .Filter(filterObservable)
            .Transform(p => new AvailableAccountViewModel(p))
            .Bind(out _accounts)
            .Subscribe();
    }

    private static Func<LoggedInAccount?, bool> MakeFilter(LoggedInAccount? selected)
    {
        return l => l != selected;
    }

    public string LoginText
    {
        get
        {
            if (_loginMgr.ActiveAccount != null)
            {
                return _loginMgr.ActiveAccount.Username + " [" + _loginMgr.ActiveAccount.LoginInfo.LoginTypeDisplaySuffix + "]";
            } else {
                return Loc.GetString("No account selected");
            }
        }
    }

    public string LogoutText => _cfg.Logins.Count == 1 ? Loc.GetString("Log out") : Loc.GetString("Log out of {0}", _loginMgr.ActiveAccount?.Username);
    public string ConfigureText => _cfg.Logins.Count == 1 ? Loc.GetString("Configure Key") : Loc.GetString("Configure {0}", _loginMgr.ActiveAccount?.Username);
    public bool LogoutButtonVisible
    {
        get
        {
            return !(_loginMgr.ActiveAccount?.LoginInfo is LoginInfoKey);
        }
    }
    public bool ConfigureButtonVisible
    {
        get
        {
            return _loginMgr.ActiveAccount?.LoginInfo is LoginInfoKey;
        }
    }

    public bool AccountSwitchVisible => _cfg.Logins.Count > 1 || _loginMgr.ActiveAccount == null;
    public string AccountSwitchText => _loginMgr.ActiveAccount != null ? Loc.GetString("Switch account:") : Loc.GetString("Select account:");
    public bool AccountControlsVisible => _loginMgr.ActiveAccount != null;

    [Reactive] public bool IsDropDownOpen { get; set; }

    public async void LogoutPressed()
    {
        IsDropDownOpen = false;

        if (_loginMgr.ActiveAccount != null)
        {
            if (_loginMgr.ActiveAccount.LoginInfo is LoginInfoAccount accountInfo)
            {
                await _authApi.LogoutTokenAsync(accountInfo.Token.Token);
            }
            _cfg.RemoveLogin(_loginMgr.ActiveAccount.LoginInfo);
        }
    }

    public async void ConfigurePressed()
    {
        IsDropDownOpen = false;

        if (!TryGetWindow(out var window))
        {
            return;
        }

        if (_loginMgr.ActiveAccount != null)
        {
            if (_loginMgr.ActiveAccount.LoginInfo is LoginInfoKey keyInfo)
            {
                await new ConfigureKeyDialog(keyInfo).ShowDialog(window);
            }
        }
    }

    [UsedImplicitly]
    public void AccountButtonPressed(LoggedInAccount account)
    {
        IsDropDownOpen = false;

        _mainVm.TrySwitchToAccount(account);
    }

    public void AddAccountPressed()
    {
        IsDropDownOpen = false;

        _loginMgr.ActiveAccount = null;
    }

    private bool TryGetWindow([NotNullWhen(true)] out Window? window)
    {
        window = Control?.GetVisualRoot() as Window;
        return window != null;
    }

}

public sealed class AvailableAccountViewModel : ViewModelBase
{
    public extern string StatusText { [ObservableAsProperty] get; }

    public LoggedInAccount Account { get; }

    public AvailableAccountViewModel(LoggedInAccount account)
    {
        Account = account;

        this.WhenAnyValue<AvailableAccountViewModel, AccountLoginStatus, string, string>(p => p.Account.Status, p => p.Account.Username, p => p.Account.LoginInfo.LoginTypeDisplaySuffix)
            .Select(p => p.Item1 switch
            {
                AccountLoginStatus.Available => $"{p.Item2} - {p.Item3}",
                AccountLoginStatus.Expired => $"{p.Item2} (!) - {p.Item3}",
                _ => $"{p.Item2} (?) - {p.Item3}"
            })
            .ToPropertyEx(this, x => x.StatusText);
    }
}
