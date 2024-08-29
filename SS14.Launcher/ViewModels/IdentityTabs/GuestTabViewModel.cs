using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CodeHollow.FeedReader;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using SS14.Launcher.Api;
using SS14.Launcher.Localization;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.Utility;
using SS14.Launcher.ViewModels.Login;

namespace SS14.Launcher.ViewModels.IdentityTabs;

public class GuestTabViewModel : IdentityTabViewModel, IErrorOverlayOwner
{
    private readonly AuthApi _authApi;
    private readonly LoginManager _loginMgr;

    [Reactive] public string EditingUsername { get; set; } = "";

    [Reactive] public bool IsInputValid { get; private set; }

    [Reactive] public string? BusyText { get; protected set; }
    [Reactive] public ViewModelBase? OverlayControl { get; set; }

    public GuestTabViewModel()
    {
        _authApi = Locator.Current.GetRequiredService<AuthApi>();
        _loginMgr = Locator.Current.GetRequiredService<LoginManager>();
        // TODO: BusyText
        // BusyText = "Logging in...";

        this.WhenAnyValue(x => x.EditingUsername)
            .Subscribe(s => { IsInputValid = !string.IsNullOrEmpty(s); });
    }

    public void SkipLoginPressed()
    {
        // Registration is purely via website now, sorry.
        //Helpers.OpenUri(ConfigConstants.AccountRegisterUrl);
        DoUnauthLogin();
    }

    private void DoUnauthLogin()
    {
        string username = EditingUsername.Trim();
        if (String.IsNullOrWhiteSpace(username) || username.Length == 0)
        {
            this.OverlayControl = new AuthErrorsOverlayViewModel(this, Loc.GetParticularString("Guest Mode Auth Error - Header", "Username needed"),
                 new string[]{Loc.GetParticularString("Guest Mode Auth Error - Description", "Even though no account will be created, servers will still need a username to call you by.  Please enter a username in the username field.")});
            return;
        }

        if (!username.All(x => char.IsLetterOrDigit(x) || x == '_'))
        {
            this.OverlayControl = new AuthErrorsOverlayViewModel(this, Loc.GetParticularString("Guest Mode Auth Error - Header", "Username bad characters"),
                 new string[]{Loc.GetParticularString("Guest Mode Auth Error - Description", "Username can only contain 0-9 a-z A-Z and _")});
            return;
        }

        var loginInfo = new LoginInfoGuest();
        loginInfo.Username = EditingUsername;


        _loginMgr.AddFreshLogin(loginInfo);
        _loginMgr.ActiveAccount = _loginMgr.GetLoggedInAccountByLoginInfo(loginInfo);
    }

    public virtual void OverlayOk()
    {
        OverlayControl = null;
    }

    public override void Selected()
    {
        base.Selected();
    }

    public override string Name => Loc.GetParticularString("Create Identity Tab", "Guest");
}
