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

    public string WelcomeText
    {
        get
        {
            return Loc.GetString(@"Let's set up your identity!

From this screen, you can create an identity on one of the established authentication providers, or just use a guest login.

Different servers accept different ways to authenticate you, and not all servers accept all authentication providers.

It is possible to manage multiple identities using this launcher, just revisit this page to establish them as needed.  (Though you probably do not want to create more than one identity per provider).

Start by choosing an identity provider on the left.  You can revisit this screen later to create identities at other providers, should you wish to do so.");
        }
    }

}
