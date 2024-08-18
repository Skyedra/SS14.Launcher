using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Localization;

namespace SS14.Launcher.Models.Data;

public class LoginInfoGuest : LoginInfo
{
    public override string ToString()
    {
        return $"{Username} [Guest]";
    }

    public override string LoginTypeDisplaySuffix => Loc.GetParticularString("Account Type", "Guest");
}
