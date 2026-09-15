using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Controls.Templates;
using Avalonia.Layout;

namespace Euterpe.Headless.Tests.Controls;

[TestSubject(typeof(WrapVirtualizer))]
[Category("WrapVirtualizerTests")]
public sealed class WrapVirtualizerTest : HeadlessTest
{
    private static (WrapVirtualizer Wrap, Window Window) CreateHost(int itemCount, double itemWidth, double windowWidth, double windowHeight) =>
        CreateHost(Enumerable.Range(0, itemCount).Select(i => $"item {i}").ToList(), itemWidth, windowWidth, windowHeight);

    private static (WrapVirtualizer Wrap, Window Window) CreateHost(IEnumerable items, double itemWidth, double windowWidth, double windowHeight)
    {
        var wrap = new WrapVirtualizer
        {
            ItemsSource = items,
            ItemWidth = itemWidth,
            ItemTemplate = new FuncDataTemplate<string>((_, _) => new Border { Width = itemWidth, Height = 40 }, true)
        };
        var window = new Window { Content = wrap, Width = windowWidth, Height = windowHeight };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (wrap, window);
    }

    private static string FlatString(WrapVirtualizer wrap) =>
        string.Join(',', wrap.Rows.SelectMany(row => row.Items));

    private static List<NotifyCollectionChangedAction> TrackRowActions(WrapVirtualizer wrap)
    {
        var actions = new List<NotifyCollectionChangedAction>();
        ((INotifyCollectionChanged)wrap.Rows).CollectionChanged += (_, e) => actions.Add(e.Action);
        return actions;
    }

    [Test]
    public Task Rows_ItemsExceedColumnCount_ChunksItemsIntoRows() => RunOnUI(async () =>
    {
        var (wrap, _) = CreateHost(10, 100, 350, 400);

        using var _ = Assert.Multiple();
        await Assert.That(wrap.Rows[0].Items.Count).IsEqualTo(3);
        await Assert.That(wrap.Rows.Count).IsEqualTo(4);
    });

    [Test]
    public Task Rows_ItemCountNotDivisibleByColumns_LastRowHoldsRemainder() => RunOnUI(async () =>
    {
        var (wrap, _) = CreateHost(10, 100, 350, 400);
        await Assert.That(wrap.Rows[^1].Items.Count).IsEqualTo(1);
    });

    [Test]
    public Task Virtualization_ItemsExceedViewport_RealizesFewerRowsThanTotal() => RunOnUI(async () =>
    {
        var (wrap, _) = CreateHost(2000, 100, 350, 400);

        var panel = wrap.GetVisualDescendants().OfType<VirtualizingStackPanel>().First();
        var realizedRows = panel.GetVisualChildren().OfType<Control>().Count();

        using var _ = Assert.Multiple();
        await Assert.That(wrap.Rows.Count).IsEqualTo(667);
        await Assert.That(realizedRows).IsGreaterThan(0);
        await Assert.That(realizedRows).IsLessThan(wrap.Rows.Count);
    });

    [Test]
    public Task Measure_AvailableWidthChanges_RecomputesColumns() => RunOnUI(async () =>
    {
        var (wrap, window) = CreateHost(12, 100, 350, 400);
        await Assert.That(wrap.Rows[0].Items.Count).IsEqualTo(3);

        window.Width = 520;
        Dispatcher.UIThread.RunJobs();

        await Assert.That(wrap.Rows[0].Items.Count).IsEqualTo(5);
    });

    [Test]
    public Task ItemTemplate_BorderTemplate_RendersItemsInRows() => RunOnUI(async () =>
    {
        var (wrap, _) = CreateHost(6, 100, 350, 400);

        var borders = wrap.GetVisualDescendants().OfType<Border>().Count(b => b is { Width: 100, Height: 40 });
        await Assert.That(borders).IsGreaterThan(0);
    });

    [Test]
    public Task Columns_ComputedFromAvailableWidth_NotArrangedWidth() => RunOnUI(async () =>
    {
        var wrap = new WrapVirtualizer
        {
            ItemsSource = Enumerable.Range(0, 10).Select(i => $"item {i}").ToList(),
            ItemWidth = 100,
            ItemTemplate = new FuncDataTemplate<string>((_, _) => new Border { Width = 100, Height = 40 }, true)
        };
        var host = new ContentControl
        {
            Content = wrap,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Top,
            Width = 350
        };
        var window = new Window { Content = host, Width = 600, Height = 400 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        await Assert.That(wrap.Rows[0].Items.Count).IsEqualTo(3);
    });

    [Test]
    public Task Arrange_CardsInSameRow_PlacesCardsHorizontally() => RunOnUI(async () =>
    {
        var (wrap, _) = CreateHost(6, 100, 350, 400);

        var cards = wrap.GetVisualDescendants()
            .OfType<Border>()
            .Where(b => b is { Width: 100, Height: 40 })
            .Take(2)
            .ToList();
        var first = cards[0].TranslatePoint(default, wrap) ?? default;
        var second = cards[1].TranslatePoint(default, wrap) ?? default;

        using var _ = Assert.Multiple();
        await Assert.That(second.X).IsGreaterThan(first.X);
        await Assert.That(second.Y).IsEqualTo(first.Y);
    });

    [Test]
    public Task Add_ItemAtEnd_AppendsRowWithoutReset() => RunOnUI(async () =>
    {
        var source = new ObservableCollection<string>(Enumerable.Range(0, 9).Select(i => $"item {i}"));
        var (wrap, _) = CreateHost(source, 100, 350, 400);
        var actions = TrackRowActions(wrap);

        source.Add("item 9");

        using var _ = Assert.Multiple();
        await Assert.That(wrap.Rows.Count).IsEqualTo(4);
        await Assert.That(FlatString(wrap)).IsEqualTo(string.Join(',', source));
        await Assert.That(actions.Contains(NotifyCollectionChangedAction.Reset)).IsFalse();
    });

    [Test]
    public Task Add_ItemInMiddle_RipplesAndPreservesOrder() => RunOnUI(async () =>
    {
        var source = new ObservableCollection<string>(Enumerable.Range(0, 10).Select(i => $"item {i}"));
        var (wrap, _) = CreateHost(source, 100, 350, 400);

        source.Insert(1, "inserted");

        await Assert.That(FlatString(wrap)).IsEqualTo(string.Join(',', source));
    });

    [Test]
    public Task Remove_Item_RipplesAndDropsEmptyRowWithoutReset() => RunOnUI(async () =>
    {
        var source = new ObservableCollection<string>(Enumerable.Range(0, 10).Select(i => $"item {i}"));
        var (wrap, _) = CreateHost(source, 100, 350, 400);
        var actions = TrackRowActions(wrap);

        source.RemoveAt(2);

        using var _ = Assert.Multiple();
        await Assert.That(wrap.Rows.Count).IsEqualTo(3);
        await Assert.That(FlatString(wrap)).IsEqualTo(string.Join(',', source));
        await Assert.That(actions.Contains(NotifyCollectionChangedAction.Reset)).IsFalse();
    });

    [Test]
    public Task Replace_Item_UpdatesSingleCellWithoutReset() => RunOnUI(async () =>
    {
        var source = new ObservableCollection<string>(Enumerable.Range(0, 10).Select(i => $"item {i}"));
        var (wrap, _) = CreateHost(source, 100, 350, 400);
        var actions = TrackRowActions(wrap);

        source[4] = "changed";

        using var _ = Assert.Multiple();
        await Assert.That(wrap.Rows[1].Items[1]).IsEqualTo("changed");
        await Assert.That(FlatString(wrap)).IsEqualTo(string.Join(',', source));
        await Assert.That(actions.Contains(NotifyCollectionChangedAction.Reset)).IsFalse();
    });

    [Test]
    public Task Move_Item_ReordersWithoutReset() => RunOnUI(async () =>
    {
        var source = new ObservableCollection<string>(Enumerable.Range(0, 10).Select(i => $"item {i}"));
        var (wrap, _) = CreateHost(source, 100, 350, 400);
        var actions = TrackRowActions(wrap);

        source.Move(0, 5);

        using var _ = Assert.Multiple();
        await Assert.That(FlatString(wrap)).IsEqualTo(string.Join(',', source));
        await Assert.That(actions.Contains(NotifyCollectionChangedAction.Reset)).IsFalse();
    });
}
