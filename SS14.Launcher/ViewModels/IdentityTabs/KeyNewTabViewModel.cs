using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
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
using TerraFX.Interop.Windows;

namespace SS14.Launcher.ViewModels.IdentityTabs;

public class KeyNewTabViewModel : IdentityTabViewModel, IErrorOverlayOwner
{
    private readonly AuthApi _authApi;
    private readonly LoginManager _loginMgr;

    [Reactive] public string EditingUsername { get; set; } = "";

    [Reactive] public bool IsInputValid { get; private set; }

    [Reactive] public string? BusyText { get; protected set; }
    [Reactive] public ViewModelBase? OverlayControl { get; set; }

    public KeyNewTabViewModel()
    {
        _authApi = Locator.Current.GetRequiredService<AuthApi>();
        _loginMgr = Locator.Current.GetRequiredService<LoginManager>();
        // TODO: BusyText
        // BusyText = "Logging in...";

        this.WhenAnyValue(x => x.EditingUsername)
            .Subscribe(s => { IsInputValid = !string.IsNullOrEmpty(s); });
    }

    public void ContinuePressed()
    {
        CreateNewKeypair();
    }

    private void CreateNewKeypair()
    {
        // First verify username
        string username = EditingUsername.Trim();
        if (String.IsNullOrWhiteSpace(username) || username.Length == 0)
        {
            this.OverlayControl = new AuthErrorsOverlayViewModel(this, Loc.GetParticularString("Guest Mode Auth Error - Header", "Username needed"), // can re-use existing loc
                 new string[]{Loc.GetParticularString("Key Auth Error - Description", "Servers will need a username to call you by.  Please enter a username in the username field.")});
            return;
        }

        if (!username.All(x => char.IsLetterOrDigit(x) || x == '_'))
        {
            this.OverlayControl = new AuthErrorsOverlayViewModel(this, Loc.GetParticularString("Guest Mode Auth Error - Header", "Username bad characters"), // can re-use existing loc
                 new string[]{Loc.GetParticularString("Guest Mode Auth Error - Description", "Username can only contain 0-9 a-z A-Z and _")}); // can re-use existing loc
            return;
        }

        // Username ok, generate keypair

        var privateKey = ECDsa.Create();
        var privateKeyString = privateKey.ExportECPrivateKeyPem();
        var publicKeyString = privateKey.ExportSubjectPublicKeyInfoPem();

        var loginInfoKey = new LoginInfoKey();
        loginInfoKey.PrivateKey = privateKeyString;
        loginInfoKey.PublicKey = publicKeyString;
        loginInfoKey.Username = username;

        _loginMgr.AddFreshLogin(loginInfoKey);
        _loginMgr.ActiveAccount = _loginMgr.GetLoggedInAccountByLoginInfo(loginInfoKey);
    }

    public virtual void OverlayOk()
    {
        OverlayControl = null;
    }

    public override void Selected()
    {
        base.Selected();
    }

    public override string Name => Loc.GetParticularString("Create Identity Tab", "⭐ Key Auth (New) ⭐");
}
