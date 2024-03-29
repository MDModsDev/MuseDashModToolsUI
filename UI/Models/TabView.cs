using MuseDashModToolsUI.ViewModels;

namespace MuseDashModToolsUI.Models;

public partial class TabView : ObservableObject
{
    public readonly string Name;
    [ObservableProperty] private string _displayName;
    public ViewModelBase ViewModel { get; set; }

    public TabView(ViewModelBase viewModel, string displayName, string name)
    {
        ViewModel = viewModel;
        DisplayName = displayName;
        Name = name;
    }
}