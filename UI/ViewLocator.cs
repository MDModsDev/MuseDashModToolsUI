using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using MuseDashModToolsUI.ViewModels;

namespace MuseDashModToolsUI;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        var name = data?.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name!);

        if (type is not null)
            return (Control)Activator.CreateInstance(type)!;
        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data) => data is ViewModelBase;
}