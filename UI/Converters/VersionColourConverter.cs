using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Converters;

public class VersionColourConverter : IValueConverter
{
    private readonly IBrush Default = "#bbb".ToBrush();
    private readonly IBrush Blue = "#82aaff".ToBrush();
    private readonly IBrush Red = "#fd2617".ToBrush();
    private readonly IBrush Purple = "#a000e6".ToBrush();
    private readonly IBrush Yellow = "#e19600".ToBrush();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Mod mod)
        {
            if (mod.IsIncompatible)
                return Red;
            switch (mod.State)
            {
                case UpdateState.Outdated:
                    return Blue;
                case UpdateState.Newer:
                    return Purple;
                case UpdateState.Modified:
                    return Yellow;
                case UpdateState.Normal:
                default:
                {
                    return Default;
                }
            }
        }

        return Default;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}