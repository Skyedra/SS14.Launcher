using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Localization;
using SS14.Launcher.Models.Data;
using SS14.Launcher.ViewModels;
using static SS14.Launcher.ViewModels.HubSettingsViewModel;

namespace SS14.Launcher.Views;

public partial class ConfigureKeyDialog : Window
{
    private readonly ConfigureKeyViewModel _viewModel;

    /// <summary>
    /// Avalonia seems to require default constructor to compile.  Not intended to be called though (perhaps there is
    /// some more elegant way of handling this?)
    /// </summary> <summary>
    public ConfigureKeyDialog()
    {
        InitializeComponent();

#if DEBUG
        this.AttachDevTools();
#endif

        _viewModel = (DataContext as ConfigureKeyViewModel)!; // Should have been set in XAML
    }

    /// <summary>
    /// Construct popup window to manage a given key
    /// </summary>
    /// <param name="keyInfo"></param>
    public ConfigureKeyDialog(LoginInfoKey? loginInfoKey)
        :this()
    {
        if (_viewModel != null)
        {
            _viewModel.LoginInfoKey = loginInfoKey;
            _viewModel.Dialog = this;
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
    }

    private void ClosePressed(object? sender, RoutedEventArgs args)
    {
        Close();
    }
}
