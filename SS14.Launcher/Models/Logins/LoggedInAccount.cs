using System;
using ReactiveUI;
using SS14.Launcher.Models.Data;

namespace SS14.Launcher.Models.Logins;

public abstract class LoggedInAccount : ReactiveObject
{
    public string Username => LoginInfo.Username;
    public string AuthServer => LoginInfo.AuthServer;
    public Guid UserId => LoginInfo.UserId;

    protected LoggedInAccount(LoginInfo loginInfo)
    {
        LoginInfo = loginInfo;
    }

    public LoginInfo LoginInfo { get; }

    public abstract AccountLoginStatus Status { get; }
}
