using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.Models;
using MuseDashModToolsUI.Contracts;

namespace MuseDashModToolsUI.Services;

public class DialogueService : IDialogueService
{
    public async Task<ButtonResult> CreateMessageBox(string title, string content, ButtonEnum button = ButtonEnum.Ok, Icon icon = Icon.Success)
    {
        var desktop = Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var isMainWindow = desktop?.MainWindow is not null;
        var messageBox = MessageBoxManager
            .GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ContentTitle = title,
                ContentMessage = content,
                ButtonDefinitions = button,
                Icon = icon,
                Topmost = true,
                WindowStartupLocation = isMainWindow ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen
            });
        return isMainWindow ? await messageBox.Show(desktop!.MainWindow) : await messageBox.Show();
    }

    public async Task<ButtonResult> CreateErrorMessageBox(string title, string content) => await CreateMessageBox(title, content, icon: Icon.Error);

    public async Task<ButtonResult> CreateErrorMessageBox(string content) => await CreateErrorMessageBox("Failure", content);

    public async Task<bool> CreateConfirmMessageBox(string title, string content)
    {
        var result = await CreateMessageBox(title, content, ButtonEnum.YesNo, Icon.Warning);
        return result.HasFlag(ButtonResult.Yes) && !result.HasFlag(ButtonResult.None);
    }

    public async Task<bool> CreateConfirmMessageBox(string content) => await CreateConfirmMessageBox("Warning", content);

    public async Task<string> CreateCustomMessageBox(string title, string content, ButtonDefinition[] buttonDefinitions, Icon icon)
    {
        var desktop = Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var isMainWindow = desktop?.MainWindow is not null;
        var messageBox = MessageBoxManager
            .GetMessageBoxCustomWindow(new MessageBoxCustomParams
            {
                ContentTitle = title,
                ContentMessage = content,
                ButtonDefinitions = buttonDefinitions,
                Icon = icon,
                CanResize = true,
                Topmost = true,
                WindowStartupLocation = isMainWindow ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen
            });
        return isMainWindow ? await messageBox.ShowDialog(desktop!.MainWindow) : await messageBox.Show();
    }

    public async Task<string> CreateCustomConfirmMessageBox(string content) =>
        await CreateCustomMessageBox(
            "Notice",
            content,
            new[]
            {
                new ButtonDefinition { Name = "Yes", IsDefault = true },
                new ButtonDefinition { Name = "No", IsCancel = true },
                new ButtonDefinition { Name = "No and Don't Ask Again", IsCancel = true }
            },
            Icon.Info);
}