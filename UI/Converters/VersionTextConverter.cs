using System;
using System.Globalization;
using Avalonia.Data.Converters;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Converters;

public class VersionTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Mod mod)
        {
            if (mod.IsIncompatible)
                return $"{mod.Name}'s {mod.Version} version is incompatible with your game version";
            switch (mod.State)
            {
                case UpdateState.Outdated:
                    return $"Local Version: {mod.LocalVersion} (Has a newer version: {mod.Version})";
                case UpdateState.Newer:
                    return $"Local Version: {mod.LocalVersion} (WOW MOD DEV)";
                case UpdateState.Modified:
                    return $"Local Version: {mod.LocalVersion} (Modified)";
                case UpdateState.Normal:
                default:
                {
                    return mod is { State: UpdateState.Normal, IsLocal: true } ? $"Local Version: {mod.LocalVersion}" : null;
                }
            }
        }

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}