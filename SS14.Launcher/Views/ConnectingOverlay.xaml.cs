using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SS14.Launcher.ViewModels;
using Avalonia.Threading;

namespace SS14.Launcher.Views;

public partial class ConnectingOverlay : UserControl
{
    public ConnectingOverlay()
    {
        InitializeComponent();
        ConnectingViewModel.StartedConnecting += () => Dispatcher.UIThread.Post(() =>
        {
            CancelButton.Focus();
            Messages.Refresh();
        });
    }
}
