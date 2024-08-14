using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace SS14.Launcher.Models.Data;

public class LoginInfoKey : LoginInfo
{
    public string PublicKey { get; set; } = default!;
    public string PrivateKey { get; set; } = default!;

    public override string ToString()
    {
        return $"{Username} [Key Auth]";
    }

    public override string LoginTypeDisplaySuffix => "MV Key Auth";
}
