using System;
using System.Runtime.Serialization;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace SS14.Launcher.Models.Data;

[DataContract]
public abstract class LoginInfo : ReactiveObject
{
    [Reactive]
    [DataMember] // (Serialize during JSON Export)
    public string Username { get; set; } = default!;

    public override string ToString()
    {
        return $"{Username} [Unknown]";
    }

    [IgnoreDataMember]
    public virtual string LoginTypeDisplaySuffix => "Unknown";
}
