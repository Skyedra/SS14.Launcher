using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace SS14.Launcher.Localization.Xaml;

public class GetExtension : MarkupExtension
{
    public GetExtension(string key)
    {
        this.Key = key;
    }

    public string Key { get; set; }

    public string Context { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (Context != null)
            return Loc.GetParticularStringWithFallback(Context, Key);

        return Loc.GetString(Key);
    }
}
