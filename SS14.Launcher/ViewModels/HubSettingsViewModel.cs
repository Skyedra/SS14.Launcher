using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DynamicData;
using Splat;
using SS14.Launcher.Localization;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Utility;

namespace SS14.Launcher.ViewModels;

public class HubSettingsViewModel : ViewModelBase
{
    public ObservableCollection<HubViewModel> HubList { get; set; } = new();

    private readonly DataManager _dataManager =  Locator.Current.GetRequiredService<DataManager>();

    public void Save()
    {
        var hubs = new List<Hub>();

        for (var i = 0; i < HubList.Count; i++)
        {
            var uri = new Uri(HubList[i].Address, UriKind.Absolute);

            // Automatically add trailing slashes for the user
            if (!uri.AbsoluteUri.EndsWith("/"))
            {
                uri = new Uri(uri.AbsoluteUri + "/", UriKind.Absolute);
            }

            hubs.Add(new Hub(uri, i));
        }

        _dataManager.SetHubs(hubs);
    }

    public void Populate()
    {
        HubList.AddRange(_dataManager.Hubs.OrderBy(h => h.Priority)
            .Select(h => new HubViewModel(h.Address.AbsoluteUri, this)));
    }

    public void Add()
    {
        HubList.Add(new HubViewModel("", this));
    }

    public void Reset()
    {
        HubList.Clear();
        foreach (var url in ConfigConstants.DefaultHubUrls)
        {
            HubList.Add(new HubViewModel(url, this));
        }
    }

    public List<string> GetDupes()
    {
        return HubList.GroupBy(h => NormalizeHubUri(h.Address))
            .Where(group => group.Count() > 1)
            .Select(x => x.Key)
            .ToList();
    }

    public static bool IsValidHubUri(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    [return: NotNullIfNotNull(nameof(address))]
    public static string? NormalizeHubUri(string? address)
    {
        if (!Uri.TryCreate(address, UriKind.Absolute, out var uri))
            return address;

        if (!uri.AbsoluteUri.EndsWith('/'))
        {
            return uri.AbsoluteUri + '/';
        }

        return uri.AbsoluteUri;
    }

    public string InfoButtonToolTip => Loc.GetParticularString("Hub Settings Info Tooltip",
@"Here you can add extra hubs to fetch game servers from.
You should only add hubs that you trust, as they can 'spoof' game servers
from other hubs. The order of the hubs matters; if two hubs advertise the
same game server the hub with the higher priority (higher in the list) will
take precedence.");
}

public class HubViewModel : ViewModelBase
{
    public string Address { get; set; }
    private readonly HubSettingsViewModel _parentVm;

    public HubViewModel(string address, HubSettingsViewModel parentVm)
    {
        Address = address;
        _parentVm = parentVm;
    }

    public void Remove()
    {
        _parentVm.HubList.Remove(this);
    }

    public void Up()
    {
        var i = _parentVm.HubList.IndexOf(this);

        if (i == 0)
            return;

        _parentVm.HubList[i] = _parentVm.HubList[i - 1];
        _parentVm.HubList[i - 1] = this;
    }

    public void Down()
    {
        var i = _parentVm.HubList.IndexOf(this);

        if (i == _parentVm.HubList.Count - 1)
            return;

        _parentVm.HubList[i] = _parentVm.HubList[i + 1];
        _parentVm.HubList[i + 1] = this;
    }
}
