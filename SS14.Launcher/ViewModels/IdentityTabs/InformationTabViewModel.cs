using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CodeHollow.FeedReader;
using ReactiveUI;
using SS14.Launcher.Localization;

namespace SS14.Launcher.ViewModels.IdentityTabs;

public class InformationTabViewModel : IdentityTabViewModel
{
    public InformationTabViewModel()
    {
    }

    public override void Selected()
    {
        base.Selected();
    }

    public override string Name => Loc.GetParticularString("Create Identity Tab", "(Information)");
}
