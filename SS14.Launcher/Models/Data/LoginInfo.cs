using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace SS14.Launcher.Models.Data;

public abstract class LoginInfo : ReactiveObject
{
    [Reactive]
    public string Username { get; set; } = default!;

    public override string ToString()
    {
        return $"{Username} [Unknown]";
    }

    public virtual string LoginTypeDisplaySuffix => "Unknown";
}
