using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Api;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.ViewModels.IdentityTabs;

namespace SS14.Launcher.Views.IdentityTabs;
public partial class KeyImportTabView : UserControl
{
    private KeyImportTabViewModel _viewModel;

    public KeyImportTabView()
    {
        InitializeComponent();

        _viewModel = (DataContext as KeyImportTabViewModel)!; // Should have been set in XAML
        if (_viewModel != null)
        {
            _viewModel.KeyImportTabView = this;
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.KeyImportTabView = null;
        }

        _viewModel = DataContext as KeyImportTabViewModel;

        if (_viewModel != null)
        {
            _viewModel.KeyImportTabView = this;
        }

        base.OnDataContextChanged(e);
    }
}
