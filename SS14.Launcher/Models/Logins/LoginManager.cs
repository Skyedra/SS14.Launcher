using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using SS14.Launcher.Api;
using SS14.Launcher.Models.Data;

namespace SS14.Launcher.Models.Logins;

// This is different from DataManager in that this class actually manages logic more complex than raw storage.
// Checking and refreshing tokens, marking accounts as "need signing in again", etc...
public sealed class LoginManager : ReactiveObject
{
    // TODO: If the user tries to connect to a server or such
    // on the split second interval that the launcher does a token refresh
    // (once a week, if you leave it open for long).
    // there is a possibility the token used by said action will be invalid because it's actively being replaced
    // oh well.
    // Do I really care to fix that?

    private readonly DataManager _cfg;
    private readonly AuthApi _authApi;

    private IDisposable? _timer;

    private readonly IObservableList<ActiveLoginData> _logins;

    public LoggedInAccount? ActiveAccount
    {
        get
        {
            return _activeAccount;
        }

        set
        {
            this.RaiseAndSetIfChanged(ref _activeAccount, value);

            if (value != null)
                _cfg.SelectedLoginInfo = value.LoginInfo;
            else
                _cfg.SelectedLoginInfo = null;
        }
    }
    private LoggedInAccount? _activeAccount;

    public IObservableList<LoggedInAccount> Logins { get; }

    public LoginManager(DataManager cfg, AuthApi authApi)
    {
        _cfg = cfg;
        _authApi = authApi;

        _logins = _cfg.Logins
            .Connect()
            .Transform(p => new ActiveLoginData(p))
            .OnItemRemoved(p =>
            {
                if (ActiveAccount != null && p.LoginInfo == ActiveAccount.LoginInfo)
                {
                    ActiveAccount = null;
                }
            })
            .AsObservableList();

        Logins = _logins
            .Connect()
            .Transform((data, guid) => (LoggedInAccount) data)
            .AsObservableList();
    }

    public async Task Initialize()
    {
        // Set up timer so that if the user leaves their launcher open for a month or something
        // his tokens don't expire.
        _timer = DispatcherTimer.Run(() =>
        {
            async void Impl()
            {
                await RefreshAllTokens();
            }

            Impl();
            return true;
        }, ConfigConstants.TokenRefreshInterval, DispatcherPriority.Background);

        // Refresh all tokens we got.
        await RefreshAllTokens();
    }

    private async Task RefreshAllTokens()
    {
        Log.Debug("Refreshing all tokens.");

        await Task.WhenAll(_logins.Items.Select(async l =>
        {
            if (l.Status == AccountLoginStatus.Expired)
            {
                // Literally don't even bother we already know it's dead and the user has to solve it.
                Log.Debug("Token for {login} is already expired", l.LoginInfo);
                return;
            }

            if (l.LoginInfo is LoginInfoAccount accountInfo && accountInfo.Token.IsTimeExpired())
            {
                // Oh hey, time expiry.
                Log.Debug("Token for {login} expired due to time", l.LoginInfo);
                l.SetStatus(AccountLoginStatus.Expired);
                return;
            }

            try
            {
                await UpdateSingleAccountStatus(l);
            }
            catch (AuthApiException e)
            {
                // TODO: Maybe retry to refresh tokens sooner if an error occured.
                // Ignore, I guess.
                Log.Warning(e, "AuthApiException while trying to refresh token for {login}", l.LoginInfo);
            }
        }));
    }

    /// <summary>
    /// Queries active logins and returns the one that contains the LoginInfo.
    /// Works for all login types.
    /// </summary>
    private ActiveLoginData? GetActiveLoginDataByLoginInfo(LoginInfo loginInfo)
    {
        return (ActiveLoginData?) GetLoggedInAccountByLoginInfo(loginInfo);
    }

    /// <summary>
    /// Queries active logins and returns the one that contains the LoginInfo.
    /// Works for all login types.
    /// </summary>
    public LoggedInAccount? GetLoggedInAccountByLoginInfo(LoginInfo loginInfo)
    {
        foreach (var activeLogin in _logins.Items)
        {
            if (activeLogin.LoginInfo == loginInfo)
                return activeLogin;
        }

        return null;
    }

    /// <summary>
    /// Tries to find an existing LoginInfoAccount by guid.  Works ONLY for Account type logins.
    /// </summary>
    public LoggedInAccount? GetLoggedInAccountByAccountLoginGuid(Guid guid)
    {
        foreach (var activeLogin in _logins.Items)
        {
            if (activeLogin.LoginInfo is LoginInfoAccount accountInfo && accountInfo.UserId == guid)
                return activeLogin;
        }

        return null;
    }

    public void AddFreshLogin(LoginInfo info)
    {
        _cfg.AddLogin(info);

       GetActiveLoginDataByLoginInfo(info)?.SetStatus(AccountLoginStatus.Available);
    }

    public void UpdateToNewToken(LoggedInAccount account, LoginToken token)
    {
        var cast = (ActiveLoginData) account;
        cast.SetStatus(AccountLoginStatus.Available);
        if (account.LoginInfo is LoginInfoAccount accountInfo)
            accountInfo.Token = token;
    }

    /// <exception cref="AuthApiException">Thrown if an API error occured.</exception>
    public Task UpdateSingleAccountStatus(LoggedInAccount account)
    {
        return UpdateSingleAccountStatus((ActiveLoginData) account);
    }

    private async Task UpdateSingleAccountStatus(ActiveLoginData data)
    {
        if (data.LoginInfo is LoginInfoAccount accountInfo)
        {
            if (accountInfo.Token.ShouldRefresh())
            {
                Log.Debug("Refreshing token for {login}", data.LoginInfo);
                // If we need to refresh the token anyways we'll just
                // implicitly do the "is it still valid" with the refresh request.
                var newTokenHopefully = await _authApi.RefreshTokenAsync(accountInfo.Token.Token);
                if (newTokenHopefully == null)
                {
                    // Token expired or whatever?
                    data.SetStatus(AccountLoginStatus.Expired);
                    Log.Debug("Token for {login} expired while refreshing it", data.LoginInfo);
                }
                else
                {
                    Log.Debug("Refreshed token for {login}", data.LoginInfo);
                    accountInfo.Token = newTokenHopefully.Value;
                    data.SetStatus(AccountLoginStatus.Available);
                }
            }
            else if (data.Status == AccountLoginStatus.Unsure)
            {
                var valid = await _authApi.CheckTokenAsync(accountInfo.Token.Token);
                Log.Debug("Token for {login} still valid? {valid}", data.LoginInfo, valid);
                data.SetStatus(valid ? AccountLoginStatus.Available : AccountLoginStatus.Expired);
            }
        } else {
            // Guest accounts & MV Key accounts we are always sure about (no server to verify against.)
            if (data.Status != AccountLoginStatus.Available)
                data.SetStatus(AccountLoginStatus.Available);
        }
    }

    private sealed class ActiveLoginData : LoggedInAccount
    {
        private AccountLoginStatus _status;

        public ActiveLoginData(LoginInfo info) : base(info)
        {
        }

        public override AccountLoginStatus Status => _status;

        public void SetStatus(AccountLoginStatus status)
        {
            this.RaiseAndSetIfChanged(ref _status, status, nameof(Status));
            Log.Debug("Setting status for login {account} to {status}", LoginInfo, status);
        }
    }
}
