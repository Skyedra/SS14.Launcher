﻿using System;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace SS14.Launcher;

public static class IconsLoader
{
    private static readonly (string path, string resource)[] Icons =
    {
        ("info-icons/discord.png", "InfoIcon-discord"),
        ("info-icons/forum.png", "InfoIcon-forum"),
        ("info-icons/github.png", "InfoIcon-github"),
        ("info-icons/web.png", "InfoIcon-web"),
        ("info-icons/wiki.png", "InfoIcon-wiki"),
    };

    public static void Load(App app)
    {
        var loader = AvaloniaLocator.Current.GetService<IAssetLoader>()!;

        foreach (var (path, resource) in Icons)
        {
            using var file = loader.Open(new Uri($"avares://SSMV.Launcher/Assets/{path}"));
            var bitmap = new Bitmap(file);
            app.Resources.Add(resource, bitmap);
        }
    }
}
