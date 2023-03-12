using System.Threading.Tasks;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.Models;

namespace MuseDashModToolsUI.Contracts;

public interface IDialogueService
{
    Task<ButtonResult> CreateMessageBox(string title, string content, ButtonEnum button = ButtonEnum.Ok, Icon icon = Icon.Success);
    Task<ButtonResult> CreateErrorMessageBox(string title, string content);
    Task<ButtonResult> CreateErrorMessageBox(string content);
    Task<bool> CreateConfirmMessageBox(string title, string content);
    Task<bool> CreateConfirmMessageBox(string content);
    Task<string> CreateCustomMessageBox(string title, string content, ButtonDefinition[] buttonDefinitions, Icon icon);
    Task<string> CreateCustomConfirmMessageBox(string content);
}