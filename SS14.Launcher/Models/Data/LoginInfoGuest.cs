using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace SS14.Launcher.Models.Data;

public class LoginInfoGuest : LoginInfo
{
    public override string ToString()
    {
        return $"{Username} [Guest]";
    }

    public override string LoginTypeDisplaySuffix => "Guest";
}
