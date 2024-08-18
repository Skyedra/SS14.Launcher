using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Localization;

namespace SS14.Launcher.Models.Data;

[DataContract]
public class LoginInfoKey : LoginInfo
{
    [DataMember] // (Serialize during JSON Export)
    public string PublicKey { get; set; } = default!;

    [DataMember] // (Serialize during JSON Export)
    public string PrivateKey { get; set; } = default!;

    public override string ToString()
    {
        return $"{Username} [Key Auth]";
    }

    public override string LoginTypeDisplaySuffix => Loc.GetParticularString("Account Type", "MV Key Auth");
}
