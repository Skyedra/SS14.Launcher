namespace SS14.Launcher.ViewModels.IdentityTabs;

public abstract class IdentityTabViewModel : ViewModelBase
{
    public abstract string Name { get; }

    public bool IsSelected { get; set; }

    public virtual void Selected()
    {
    }
}
