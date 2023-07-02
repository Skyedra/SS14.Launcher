using System;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Api;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.Logins;
using System.Linq;

namespace SS14.Launcher.ViewModels.Login;

public class LoginViewModel : BaseLoginViewModel
{
    private readonly AuthApi _authApi;
    private readonly LoginManager _loginMgr;
    private readonly DataManager _dataManager;

    [Reactive] public string EditingUsername { get; set; } = "";
    [Reactive] public string EditingPassword { get; set; } = "";

    [Reactive] public bool IsInputValid { get; private set; }

    public LoginViewModel(MainWindowLoginViewModel parentVm, AuthApi authApi,
        LoginManager loginMgr, DataManager dataManager) : base(parentVm)
    {
        BusyText = "Logging in...";
        _authApi = authApi;
        _loginMgr = loginMgr;
        _dataManager = dataManager;

        this.WhenAnyValue(x => x.EditingUsername, x => x.EditingPassword)
            .Subscribe(s => { IsInputValid = !string.IsNullOrEmpty(s.Item1) && !string.IsNullOrEmpty(s.Item2); });
    }

    public async void OnLogInButtonPressed()
    {
        if (!IsInputValid || Busy)
        {
            return;
        }

        Busy = true;
        try
        {
            var request = new AuthApi.AuthenticateRequest(EditingUsername, EditingPassword);
            var resp = await _authApi.AuthenticateAsync(request);

            await DoLogin(this, request, resp, _loginMgr, _authApi);

            _dataManager.CommitConfig();
        }
        finally
        {
            Busy = false;
        }
    }

    public static async Task<bool> DoLogin<T>(
        T vm,
        AuthApi.AuthenticateRequest request,
        AuthenticateResult resp,
        LoginManager loginMgr,
        AuthApi authApi)
        where T : BaseLoginViewModel, IErrorOverlayOwner
    {
        if (resp.IsSuccess)
        {
            var loginInfo = resp.LoginInfo;
            var oldLogin = loginMgr.Logins.Lookup(loginInfo.UserId);
            if (oldLogin.HasValue)
            {
                // Already had this login, apparently.
                // Thanks user.
                //
                // Log the OLD token out since we don't need two of them.
                // This also has the upside of re-available-ing the account
                // if the user used the main login prompt on an account we already had, but as expired.

                await authApi.LogoutTokenAsync(oldLogin.Value.LoginInfo.Token.Token);
                loginMgr.ActiveAccountId = loginInfo.UserId;
                loginMgr.UpdateToNewToken(loginMgr.ActiveAccount!, loginInfo.Token);
                return true;
            }

            loginMgr.AddFreshLogin(loginInfo);
            loginMgr.ActiveAccountId = loginInfo.UserId;
            return true;
        }

        if (resp.Code == AuthApi.AuthenticateDenyResponseCode.TfaRequired)
        {
            vm.ParentVM.SwitchToAuthTfa(request);
            return false;
        }

        var errors = AuthErrorsOverlayViewModel.AuthCodeToErrors(resp.Errors, resp.Code);
        vm.OverlayControl = new AuthErrorsOverlayViewModel(vm, "Unable to log in", errors);
        return false;
    }

    public void RegisterPressed()
    {
        // Registration is purely via website now, sorry.
        Helpers.OpenUri(ConfigConstants.AccountRegisterUrl);
    }

    public void SkipLoginPressed()
    {
        // Registration is purely via website now, sorry.
        //Helpers.OpenUri(ConfigConstants.AccountRegisterUrl);
        DoUnauthLogin();
    }

    public void ResendConfirmationPressed()
    {
        // Registration is purely via website now, sorry.
        Helpers.OpenUri(ConfigConstants.AccountResendConfirmationUrl);
    }

    private void DoUnauthLogin()
    {
        string username = EditingUsername.Trim();
        if (String.IsNullOrWhiteSpace(username) || username.Length == 0)
        {
            this.OverlayControl = new AuthErrorsOverlayViewModel(this, "Username needed",
                new string[]{"Even though no account will be created, servers will still need a username to call you by.  Please enter a username in the username field.  (No password is needed)"});
            return;
        }

        if (!username.All(x => char.IsLetterOrDigit(x) || x == '_'))
        {
            this.OverlayControl = new AuthErrorsOverlayViewModel(this, "Username bad characters",
                new string[]{"Username can only contain 0-9 a-z A-Z and _"});
            return;
        }

        var loginInfo = new LoginInfo();
        loginInfo.UserId = Guid.NewGuid(); // Guid.Empty;
        loginInfo.Username = EditingUsername;
        loginInfo.Token = new Models.LoginToken("", DateTimeOffset.UtcNow.AddHours(2));
        loginInfo.AuthServer = LoginInfo.CommonAuthServers.Offline.ToString();

        var oldLogin = _loginMgr.Logins.Lookup(loginInfo.UserId);
        if (oldLogin.HasValue)
        {
            _loginMgr.ActiveAccountId = loginInfo.UserId;
            _loginMgr.UpdateToNewToken(_loginMgr.ActiveAccount!, loginInfo.Token);
        } else {
            _loginMgr.AddFreshLogin(loginInfo);
            _loginMgr.ActiveAccountId = loginInfo.UserId;
        }
    }
}
