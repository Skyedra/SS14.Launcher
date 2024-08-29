using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace SS14.Launcher.Models.Data;

public class LoginInfoAccount : LoginInfo
{
    [Reactive]
    public Guid UserId { get; set; }
    [Reactive]
    public LoginToken Token { get; set; }

    public enum CommonAuthServers
    {
        WizDen
    };

    [Reactive]
    public string AuthServer { get; set; }

    public override string ToString()
    {
        return $"{Username}/{UserId} [Account]";
    }

    public override string LoginTypeDisplaySuffix => AuthServer;
}
