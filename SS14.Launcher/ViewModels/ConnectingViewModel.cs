using System;
using System.Reactive.Linq;
using System.Threading;
using ReactiveUI;
using Splat;
using SS14.Launcher.Localization;
using SS14.Launcher.Models;
using SS14.Launcher.Utility;

namespace SS14.Launcher.ViewModels;

public class ConnectingViewModel : ViewModelBase
{
    private readonly Connector _connector;
    private readonly Updater _updater;
    private readonly MainWindowViewModel _windowVm;
    private readonly ConnectionType _connectionType;

    private readonly CancellationTokenSource _cancelSource = new CancellationTokenSource();

    private string? _reasonSuffix;

    private Connector.ConnectionStatus _connectorStatus;
    private Updater.UpdateStatus _updaterStatus;
    private (long downloaded, long total, Updater.ProgressUnit unit)? _updaterProgress;
    private long? _updaterSpeed;

    public bool IsErrored => _connectorStatus == Connector.ConnectionStatus.ConnectionFailed ||
                             _connectorStatus == Connector.ConnectionStatus.UpdateError ||
                             _connectorStatus == Connector.ConnectionStatus.NotAContentBundle ||
                             _connectorStatus == Connector.ConnectionStatus.ClientExited &&
                             _connector.ClientExitedBadly;

    public static event Action? StartedConnecting;

    public ConnectingViewModel(Connector connector, MainWindowViewModel windowVm, string? givenReason, ConnectionType connectionType)
    {
        _updater = Locator.Current.GetRequiredService<Updater>();
        _connector = connector;
        _windowVm = windowVm;
        _connectionType = connectionType;
        _reasonSuffix = (givenReason != null) ? ("\n" + givenReason) : "";

        this.WhenAnyValue(x => x._updater.Progress)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(progress =>
            {
                _updaterProgress = progress;

                this.RaisePropertyChanged(nameof(Progress));
                this.RaisePropertyChanged(nameof(ProgressIndeterminate));
                this.RaisePropertyChanged(nameof(ProgressText));
            });

        this.WhenAnyValue(x => x._updater.Speed)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(speed =>
            {
                _updaterSpeed = speed;

                this.RaisePropertyChanged(nameof(SpeedText));
                this.RaisePropertyChanged(nameof(SpeedIndeterminate));
            });

        this.WhenAnyValue(x => x._updater.Status)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(status =>
            {
                _updaterStatus = status;
                this.RaisePropertyChanged(nameof(StatusText));
            });

        this.WhenAnyValue(x => x._connector.Status)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(val =>
            {
                _connectorStatus = val;

                this.RaisePropertyChanged(nameof(ProgressIndeterminate));
                this.RaisePropertyChanged(nameof(StatusText));
                this.RaisePropertyChanged(nameof(ProgressBarVisible));
                this.RaisePropertyChanged(nameof(IsErrored));

                if (val == Connector.ConnectionStatus.ClientRunning
                    || val == Connector.ConnectionStatus.Cancelled
                    || val == Connector.ConnectionStatus.ClientExited && !_connector.ClientExitedBadly)
                {
                    CloseOverlay();
                }
            });

        this.WhenAnyValue(x => x._connector.ClientExitedBadly)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(StatusText));
                this.RaisePropertyChanged(nameof(IsErrored));
            });
    }

    public float Progress
    {
        get
        {
            if (_updaterProgress == null)
            {
                return 0;
            }

            var (downloaded, total, _) = _updaterProgress.Value;

            return downloaded / (float)total;
        }
    }

    public string ProgressText
    {
        get
        {
            if (_updaterProgress == null)
            {
                return "";
            }

            var (downloaded, total, unit) = _updaterProgress.Value;

            return unit switch
            {
                Updater.ProgressUnit.Bytes => $"{Helpers.FormatBytes(downloaded)} / {Helpers.FormatBytes(total)}",
                _ => $"{downloaded} / {total}"
            };
        }
    }

    public bool ProgressIndeterminate => _connectorStatus != Connector.ConnectionStatus.Updating
                                         || _updaterProgress == null;

    public bool ProgressBarVisible => _connectorStatus != Connector.ConnectionStatus.ClientExited &&
                                      _connectorStatus != Connector.ConnectionStatus.ClientRunning &&
                                      _connectorStatus != Connector.ConnectionStatus.ConnectionFailed &&
                                      _connectorStatus != Connector.ConnectionStatus.UpdateError &&
                                      _connectorStatus != Connector.ConnectionStatus.NotAContentBundle;

    public bool SpeedIndeterminate => _connectorStatus != Connector.ConnectionStatus.Updating || _updaterSpeed == null;

    public string SpeedText
    {
        get
        {
            if (_updaterSpeed is not { } speed)
                return "";

            return $"{Helpers.FormatBytes(speed)}/s";
        }
    }

    public string StatusText =>
        _connectorStatus switch
        {
            Connector.ConnectionStatus.None =>
                Loc.GetParticularString("Connecting", "Starting connection... {0}", _reasonSuffix ?? ""),
            Connector.ConnectionStatus.UpdateError =>
                Loc.GetParticularString("Connecting",
                "There was an error while downloading server content. Please ask on Discord for support if the problem persists."),
            Connector.ConnectionStatus.Updating => Loc.GetParticularString("Connecting", "Updating: {0} {1}", _updaterStatus switch
            {
                Updater.UpdateStatus.CheckingClientUpdate => Loc.GetParticularString("Connecting", "Checking for server content update..."),
                Updater.UpdateStatus.DownloadingEngineVersion => Loc.GetParticularString("Connecting", "Downloading server content..."),
                Updater.UpdateStatus.DownloadingClientUpdate => Loc.GetParticularString("Connecting", "Downloading server content..."),
                Updater.UpdateStatus.FetchingClientManifest => Loc.GetParticularString("Connecting", "Fetching server manifest..."),
                Updater.UpdateStatus.Verifying => Loc.GetParticularString("Connecting", "Verifying download integrity..."),
                Updater.UpdateStatus.CullingEngine => Loc.GetParticularString("Connecting", "Clearing old content..."),
                Updater.UpdateStatus.CullingContent => Loc.GetParticularString("Connecting", "Clearing old server content..."),
                Updater.UpdateStatus.Ready => Loc.GetParticularString("Connecting", "Update done!"),
                Updater.UpdateStatus.CheckingEngineModules => Loc.GetParticularString("Connecting", "Checking for additional dependencies..."),
                Updater.UpdateStatus.DownloadingEngineModules => Loc.GetParticularString("Connecting", "Downloading extra dependencies..."),
                Updater.UpdateStatus.CommittingDownload => Loc.GetParticularString("Connecting", "Synchronizing to disk..."),
                Updater.UpdateStatus.LoadingIntoDb => Loc.GetParticularString("Connecting", "Storing assets in database..."),
                Updater.UpdateStatus.LoadingContentBundle => Loc.GetParticularString("Connecting", "Loading content bundle..."),
                _ => Loc.GetParticularString("Connecting", "You shouldn't see this")
            }, _reasonSuffix ?? ""),
            Connector.ConnectionStatus.Connecting => Loc.GetParticularString("Connecting", "Fetching connection info from server... {0}", _reasonSuffix ?? ""),
            Connector.ConnectionStatus.ConnectionFailed => Loc.GetParticularString("Connecting", "Failed to connect to server!"),
            Connector.ConnectionStatus.StartingClient => Loc.GetParticularString("Connecting", "Starting client... {0}", _reasonSuffix ?? ""),
            Connector.ConnectionStatus.NotAContentBundle => Loc.GetParticularString("Connecting", "File is not a valid content bundle!"),
            Connector.ConnectionStatus.ClientExited => _connector.ClientExitedBadly
                ? Loc.GetParticularString("Connecting", "Client seems to have crashed while starting. If this persists, please ask on Discord or GitHub for support.")
                : "",
            _ => ""
        };

    public string TitleText => _connectionType switch
    {
        ConnectionType.Server => Loc.GetString("Connecting..."),
        ConnectionType.ContentBundle => Loc.GetString("Loading..."),
        _ => ""
    };

    public static void StartConnect(MainWindowViewModel windowVm, string address, string? givenReason = null)
    {
        var connector = new Connector();
        var vm = new ConnectingViewModel(connector, windowVm, givenReason, ConnectionType.Server);
        windowVm.ConnectingVM = vm;
        vm.Start(address);
    }

    public static void StartContentBundle(MainWindowViewModel windowVm, string fileName)
    {
        var connector = new Connector();
        var vm = new ConnectingViewModel(connector, windowVm, null, ConnectionType.ContentBundle);
        windowVm.ConnectingVM = vm;
        vm.StartContentBundle(fileName);
    }

    private void Start(string address)
    {
        _connector.Connect(address, _cancelSource.Token);
    }

    private void StartContentBundle(string fileName)
    {
        _connector.LaunchContentBundle(fileName, _cancelSource.Token);
    }

    public void ErrorDismissed()
    {
        CloseOverlay();
    }

    private void CloseOverlay()
    {
        _windowVm.ConnectingVM = null;
    }

    public void Cancel()
    {
        _cancelSource.Cancel();
    }

    public enum ConnectionType
    {
        Server,
        ContentBundle
    }
}
