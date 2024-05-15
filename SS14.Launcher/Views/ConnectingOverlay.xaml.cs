using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
