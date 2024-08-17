using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Avalonia.Controls;
using Avalonia.VisualTree;
using CodeHollow.FeedReader;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using SS14.Launcher.Api;
using SS14.Launcher.Localization;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.Utility;
using SS14.Launcher.ViewModels.Login;
using SS14.Launcher.Views.IdentityTabs;
using TerraFX.Interop.Windows;

namespace SS14.Launcher.ViewModels.IdentityTabs;

public class KeyImportTabViewModel : IdentityTabViewModel, IErrorOverlayOwner
{
    private readonly AuthApi _authApi;
    private readonly LoginManager _loginMgr;

    [Reactive] public ViewModelBase? OverlayControl { get; set; }
    public KeyImportTabView KeyImportTabView { get; set; }

    public KeyImportTabViewModel()
    {
        _authApi = Locator.Current.GetRequiredService<AuthApi>();
        _loginMgr = Locator.Current.GetRequiredService<LoginManager>();
        // TODO: BusyText
        // BusyText = "Logging in...";
    }

    public async void ImportPressed()
    {
        if (KeyImportTabView == null)
            return;

        Window rootWindow = (Window) KeyImportTabView.GetVisualRoot();
        if (rootWindow == null)
            return;

        var openFileDialog = new OpenFileDialog();
        openFileDialog.InitialFileName = "SSMV Key - THIS IS YOUR PASSWORD.key";
        openFileDialog.Directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (openFileDialog.Directory == "") // Fallback
            openFileDialog.Directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        if (openFileDialog.Directory == "") // Fallback
            openFileDialog.Directory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        openFileDialog.AllowMultiple = false;
        var selectedPaths = await openFileDialog.ShowAsync(rootWindow);
        if (selectedPaths == null || selectedPaths.Length != 1)
        {
            this.OverlayControl = new AuthErrorsOverlayViewModel(this, Loc.GetParticularString("Key Auth Error - Header", "Error"),
                 new string[]{Loc.GetParticularString("Key Auth Error - Description", "Please select exactly one keypair file to load.")});
            return;
        }

        var selectedPath = selectedPaths[0];

        if (String.IsNullOrEmpty(selectedPath))
        {
            this.OverlayControl = new AuthErrorsOverlayViewModel(this, Loc.GetParticularString("Key Auth Error - Header", "Error"),
                new string[]{Loc.GetParticularString("Key Auth Error - Description", "Please select exactly one keypair file to load.")});
            return;
        }

        try
        {
            string fileContents = File.ReadAllText(selectedPath);
            var newlyLoadedLoginKey = JsonConvert.DeserializeObject<LoginInfoKey>(fileContents);
            if (newlyLoadedLoginKey == null ||
                String.IsNullOrEmpty(newlyLoadedLoginKey.PrivateKey) ||
                String.IsNullOrEmpty(newlyLoadedLoginKey.PublicKey) ||
                String.IsNullOrEmpty(newlyLoadedLoginKey.Username))
            {
                this.OverlayControl = new AuthErrorsOverlayViewModel(this, Loc.GetParticularString("Key Auth Error - Header", "Error"),
                    new string[]{Loc.GetParticularString("Key Auth Error - Description", "Invalid SSMV JSON keypair file.")});
                return;
            }

            // If the username and publickey match, then user just tried to import the same file twice, which would be
            // pointless.  (There is mayyyybe a case for importing the same privatekey/publickey with different usernames
            // for people who like different names on different servers, but don't want to juggle multiple keypairs.)

            foreach(var login in _loginMgr.Logins.Items)
            {
                if (login.LoginInfo is LoginInfoKey testKey && testKey.PublicKey == newlyLoadedLoginKey.PublicKey &&
                    testKey.Username == newlyLoadedLoginKey.Username)
                {
                    this.OverlayControl = new AuthErrorsOverlayViewModel(this, Loc.GetParticularString("Key Auth Error - Header", "Error"),
                        new string[]{Loc.GetParticularString("Key Auth Error - Description", "You already have this key loaded ({0}).  Select it in top right account dropdown menu.", newlyLoadedLoginKey.Username)});
                    return;
                }
            }

            // Verify username
            if (String.IsNullOrWhiteSpace(newlyLoadedLoginKey.Username) || newlyLoadedLoginKey.Username.Length == 0)
            {
                this.OverlayControl = new AuthErrorsOverlayViewModel(this, Loc.GetParticularString("Guest Mode Auth Error - Header", "Username needed"), // can re-use existing loc
                    new string[]{Loc.GetParticularString("Key Auth Error - Description", "Servers will need a username to call you by.  Please enter a username in the username field.")});
                return;
            }

            if (!newlyLoadedLoginKey.Username.All(x => char.IsLetterOrDigit(x) || x == '_'))
            {
                this.OverlayControl = new AuthErrorsOverlayViewModel(this, Loc.GetParticularString("Guest Mode Auth Error - Header", "Username bad characters"), // can re-use existing loc
                    new string[]{Loc.GetParticularString("Guest Mode Auth Error - Description", "Username can only contain 0-9 a-z A-Z and _")}); // can re-use existing loc
                return;
            }

            // Username ok, verify keypair
            var privateKey = ECDsa.Create();
            privateKey.ImportFromPem(newlyLoadedLoginKey.PrivateKey);

            var publicKey = ECDsa.Create();
            publicKey.ImportFromPem(newlyLoadedLoginKey.PublicKey);

            // Meets all criteria, import it
            _loginMgr.AddFreshLogin(newlyLoadedLoginKey);
            _loginMgr.ActiveAccount = _loginMgr.GetLoggedInAccountByLoginInfo(newlyLoadedLoginKey);

        } catch (Exception e)
        {
            this.OverlayControl = new AuthErrorsOverlayViewModel(this, Loc.GetParticularString("Key Auth Error - Header", "Error"),
                new string[]{Loc.GetParticularString("Key Auth Error - Description", "Exception during import.  Did you select the right SSMV JSON keypair file?") + "\n" + e.Message});
            return;
        }
    }

    public virtual void OverlayOk()
    {
        OverlayControl = null;
    }

    public override void Selected()
    {
        base.Selected();
    }

    public override string Name => Loc.GetParticularString("Create Identity Tab", "Key Auth (Import)");


}
