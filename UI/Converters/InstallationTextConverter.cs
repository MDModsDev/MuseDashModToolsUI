using System;
using System.Globalization;
using Avalonia.Data.Converters;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Converters;

public class InstallationTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Mod mod)
        {
            switch (mod.State)
            {
                case UpdateState.Outdated:
                    return "Update";
                case UpdateState.Newer:
                case UpdateState.Modified:
                    return "Reinstall";
                case UpdateState.Normal:
                default:
                {
                    return string.Empty;
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