using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using DynamicData;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using SS14.Launcher.Localization;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.Utility;
using SS14.Launcher.Views;

namespace SS14.Launcher.ViewModels;

public class ConfigureKeyViewModel : ViewModelBase
{
    [Reactive] public string EditingUsername { get; set; } = "";
    [Reactive] public string PublicKeyText { get; set; } = "";
    private readonly LoginManager _loginMgr;
    private readonly DataManager _cfg;
    [Reactive] public bool ConfirmDeleteButtonEnabled { get; set; } = false;
    public ConfigureKeyDialog Dialog { get; set; }

    public ConfigureKeyViewModel()
        :base()
    {
        _loginMgr = Locator.Current.GetRequiredService<LoginManager>();
        _cfg = Locator.Current.GetRequiredService<DataManager>();

        this.WhenAnyValue(x => x.EditingUsername)
            .Subscribe(s => {
                this.RaisePropertyChanged(nameof(RenameButtonEnabled));
            });

        this.WhenAnyValue(x => x.RenameResultText)
            .Subscribe(s => {
                this.RaisePropertyChanged(nameof(ShowRenameResultText));
            });

        this.WhenAnyValue(x => x.ExportResultText)
            .Subscribe(s => {
                this.RaisePropertyChanged(nameof(ShowExportResultText));
            });
    }


    public bool RenameButtonEnabled
    {
        get
        {
            if (LoginInfoKey == null)
                return false;

            return EditingUsername != LoginInfoKey.Username;
        }
    }

    public LoginInfoKey? LoginInfoKey {
        get
        {
            return _loginInfoKey;
        }

        internal set
        {
            _loginInfoKey = value;
            if (_loginInfoKey == null)
            {
                EditingUsername = "";
                PublicKeyText = "";
            }
            else
            {
                EditingUsername = _loginInfoKey.Username;
                PublicKeyText = _loginInfoKey.PublicKey;
            }
            this.RaisePropertyChanged(nameof(RenameButtonEnabled));
        }
    }
    private LoginInfoKey? _loginInfoKey;

    [Reactive] public string RenameResultText { get; set; } = "";
    public bool ShowRenameResultText
    {
        get
        {
            return RenameResultText != "";
        }
    }

    public void RenamePressed()
    {
        if (String.IsNullOrWhiteSpace(EditingUsername) || EditingUsername.Length == 0)
        {
            RenameResultText = Loc.GetParticularString("Key Auth Error - Description", "Servers will need a username to call you by.  Please enter a username in the username field.");
            return;
        }

        if (!EditingUsername.All(x => char.IsLetterOrDigit(x) || x == '_'))
        {
            RenameResultText = Loc.GetParticularString("Guest Mode Auth Error - Description", "Username can only contain 0-9 a-z A-Z and _"); // can re-use existing loc
            return;
        }

        if (LoginInfoKey == null)
            return;

        LoginInfoKey.Username = EditingUsername;
        _cfg.CommitConfig();

        RenameResultText = Loc.GetParticularString("Key Configuration", "Rename was successful.");
        RenameResultText += "  " + Loc.GetParticularString("Key Configuration", "(UI may not reflect the rename until next restart of Multiverse.)"); // Temporary workaround, sorry
        this.RaisePropertyChanged(nameof(RenameButtonEnabled));
    }

    public void DeletePressed()
    {
        if (LoginInfoKey == null)
            return;

        ConfirmDeleteButtonEnabled = true;
    }

    public void DeleteConfirmPressed()
    {
        if (LoginInfoKey == null)
            return;

        _cfg.RemoveLogin(LoginInfoKey);

        Dialog.Close();
    }

    private async void ExportPressed()
    {
        if (LoginInfoKey == null || Dialog == null)
            return;

        var saveFileDialog = new SaveFileDialog();
        saveFileDialog.InitialFileName = "SSMV Key - THIS IS YOUR PASSWORD.key";
        saveFileDialog.Directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (saveFileDialog.Directory == "") // Fallback
            saveFileDialog.Directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        if (saveFileDialog.Directory == "") // Fallback
            saveFileDialog.Directory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string? selectedPath = await saveFileDialog.ShowAsync(Dialog);

        if (selectedPath == null)
        {
            ExportResultText = Loc.GetParticularString("Key Configuration", "No path selected.");
            return;
        }

        try
        {
            //string serializedForm = JsonSerializer.Serialize<LoginInfoKey>(LoginInfoKey);
            // ^ Built in .net serialization tries to serialize out Reactive fields.  Using json.net instead.
            string serializedForm = JsonConvert.SerializeObject(LoginInfoKey, Formatting.Indented);
            await File.WriteAllTextAsync(selectedPath, serializedForm);
            ExportResultText = Loc.GetParticularString("Key Configuration", "Export succeeded.");
        } catch (Exception e)
        {
            ExportResultText = Loc.GetParticularString("Key Configuration", "Error during export: ") + e.Message;
        }
    }

    [Reactive] public string ExportResultText { get; set; } = "";
    public bool ShowExportResultText
    {
        get
        {
            return ExportResultText != "";
        }
    }
}
