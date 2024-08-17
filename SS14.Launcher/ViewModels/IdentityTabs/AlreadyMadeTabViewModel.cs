using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CodeHollow.FeedReader;
using ReactiveUI;
using SS14.Launcher.Localization;

namespace SS14.Launcher.ViewModels.IdentityTabs;

public class AlreadyMadeTabViewModel : IdentityTabViewModel
{
    public AlreadyMadeTabViewModel(string name, IdentityTabViewModel replacementFor)
    {
        this._name = name;
        this.ReplacementFor = replacementFor;
    }

    public override void Selected()
    {
        base.Selected();
    }

    public override string Name
    {
        get
        {
            return _name;
        }
    }
    private string _name;

    public string WelcomeText
    {
        get
        {
            return Loc.GetString(@"You've already made an account of this type.  Select it at the top right.");
        }
    }

    /// <summary>
    /// What normal tab is this stub tab replacing?
    /// </summary>
    public IdentityTabViewModel ReplacementFor { get; set; }

}
