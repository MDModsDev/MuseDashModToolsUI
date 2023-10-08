using System.Collections.ObjectModel;
using DynamicData;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class ChartDownloadViewModel : ViewModelBase, IChartDownloadViewModel
{
    private readonly ReadOnlyObservableCollection<Chart> _charts;
    private readonly SourceCache<Chart, string> _sourceCache = new(x => x.Name);
    [ObservableProperty] private int _currentSortOptionIndex;
    [ObservableProperty] private string _filter;
    [ObservableProperty] private string[] _sortOptions;
    public ReadOnlyObservableCollection<Chart> Charts => _charts;

    [UsedImplicitly]
    public IChartService ChartService { get; init; }

    [UsedImplicitly]
    public ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    public ChartDownloadViewModel()
    {
        _sourceCache.Connect()
            .Filter(x => string.IsNullOrEmpty(Filter)
                         || x.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase)
                         || x.Author.Contains(Filter, StringComparison.OrdinalIgnoreCase)
                         || x.Charter.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            .SortBy(GetSortByOption)
            .Bind(out _charts)
            .Subscribe();
    }

    public ChartSortOptions SortOption { get; private set; }

    public async Task Initialize()
    {
        SortOptions = new[]
        {
            XAML_ChartSortOption_Default, XAML_ChartSortOption_Name,
            XAML_ChartSortOption_Downloads, XAML_ChartSortOption_Likes,
            XAML_ChartSortOption_Level, XAML_ChartSortOption_Latest
        };
        await ChartService.InitializeChartList(_sourceCache, Charts);
        Logger.Information("Chart Download Window Initialized");
    }

    private IComparable GetSortByOption(Chart chart)
    {
        return SortOption switch
        {
            ChartSortOptions.Id => chart.Id,
            ChartSortOptions.Name => chart.Name,
            ChartSortOptions.Downloads => -chart.Analytics.Downloads,
            ChartSortOptions.Likes => -chart.Analytics.LikesCount,
            ChartSortOptions.Level => -chart.GetHighestLevel(),
            ChartSortOptions.Latest => -chart.Id,
            _ => ChartSortOptions.Id
        };
    }

    [UsedImplicitly]
    partial void OnCurrentSortOptionIndexChanged(int value)
    {
        if (value == -1) return;
        SortOption = (ChartSortOptions)value;
        _sourceCache.Refresh();
    }

    [UsedImplicitly]
    partial void OnFilterChanged(string value) => _sourceCache.Refresh();

    #region Commands

    [RelayCommand]
    private async Task DownloadChart(Chart item) => await ChartService.DownloadChart(item);

    [RelayCommand]
    private async Task OpenCustomAlbumsFolder() => await LocalService.OpenCustomAlbumsFolder();

    #endregion
}