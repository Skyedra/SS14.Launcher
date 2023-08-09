using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CodeHollow.FeedReader;
using ReactiveUI;

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

    public override string Name => "(Information)";
}
