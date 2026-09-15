using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Labs.AnimatedImage;
using Euterpe.Abstractions;

namespace Euterpe.Headless.Tests.Controls;

[TestSubject(typeof(CoverImage))]
[Category("CoverImageTests")]
public sealed class CoverImageTest : HeadlessTest
{
    private static readonly byte[] MinimalAnimatedWebp =
        Convert.FromBase64String("UklGRoQAAABXRUJQVlA4WAoAAAACAAAAAQAAAQAAQU5JTQYAAAAAAAAAAABBTk1GKAAAAAAAAAAAAAEAAAEAAGQAAAJWUDhMDwAAAC8BQAAA"
                                + "BxD9j/4HIqL/AQBBTk1GKAAAAAAAAAAAAAEAAAEAAMgAAAJWUDhMDwAAAC8BQAAABxDR//4HIqL/AQA=");

    private static readonly byte[] MinimalWebp =
        Convert.FromBase64String("UklGRjwAAABXRUJQVlA4IDAAAADQAQCdASoEAAQAAgA0JaACdLoB+AADsAD+8Oj3/yC5YXXI1/8gP+QH/ID/+PIAAAA=");

    [Test]
    public Task Stretch_NewControl_IsUniform() => RunOnUI(async () =>
    {
        var cover = new CoverImage();
        await Assert.That(cover.Stretch).IsEqualTo(Stretch.Uniform);
    });

    [Test]
    public Task ApplyTemplate_DefaultTheme_CreatesAnimatedPart() => RunOnUI(async () =>
    {
        var cover = Show(new CoverImage());

        await Assert.That(AnimatedPart(cover)).IsNotNull();
    });

    [Test]
    public Task ApplyTemplate_NullSource_DoesNotThrow() => RunOnUI(async () =>
    {
        var cover = Show(new CoverImage { Source = null });
        await Assert.That(cover.Source).IsNull();
    });

    [Test]
    public Task Source_AnimatedWebp_DecodesFramesAndShowsAnimation() => RunOnUI(async () =>
    {
        var path = CreateTempWebp(MinimalAnimatedWebp);
        try
        {
            var cover = Show(new CoverImage { Source = new Uri(path).AbsoluteUri, Width = 164, Height = 164 });
            var source = await WaitForAnimatedSource(cover);

            using var _ = Assert.Multiple();
            await Assert.That(source).IsNotNull();
            await Assert.That(source!.IsInitialized).IsTrue();
            await Assert.That(source.FrameCount).IsEqualTo(2);
            await Assert.That(source.Delays[0]).IsEqualTo(100);
            await Assert.That(source.Delays[1]).IsEqualTo(200);
            await Assert.That(AnimatedPart(cover).IsVisible).IsTrue();
            await Assert.That(cover.Bounds.Size).IsEqualTo(new Size(164, 164));
        }
        finally
        {
            DeleteTempWebp(path);
        }
    });

    [Test]
    [NotInParallel("AsyncImage.DefaultRemoteLoader")]
    public Task DefaultRemoteLoader_RemoteWebpSource_BuildsAnimatedSource() => RunOnUI(async () =>
    {
        const string uri = "https://euterpe-org.com/cover.webp";
        var previousLoader = AsyncImage.DefaultRemoteLoader;
        try
        {
            Uri? requestedSource = null;
            var loader = IRemoteImageLoader.Mock();
            loader.OpenReadAsync(Any<Uri>(), Any<CancellationToken>())
                .Callback((source, _) => requestedSource = source)
                .Returns(() => new MemoryStream(MinimalAnimatedWebp, false));
            AsyncImage.DefaultRemoteLoader = loader;
            var cover = Show(new CoverImage { Source = uri });

            var animatedSource = await WaitForAnimatedSource(cover);

            using var _ = Assert.Multiple();
            await Assert.That(animatedSource).IsNotNull();
            await Assert.That(requestedSource).IsEqualTo(new Uri(uri));
        }
        finally
        {
            AsyncImage.DefaultRemoteLoader = previousLoader;
        }
    });

    [Test]
    [NotInParallel("AsyncImage.DefaultRemoteLoader")]
    public Task SourceChanged_PendingLoad_CancelsAndKeepsLatestImage() => RunOnUI(async () =>
    {
        var previousLoader = AsyncImage.DefaultRemoteLoader;
        var firstLoad = new TaskCompletionSource<Stream?>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var firstStream = new MemoryStream(MinimalAnimatedWebp, false);
        try
        {
            var firstUri = new Uri("https://euterpe-org.com/first/cover.webp");
            var secondUri = new Uri("https://euterpe-org.com/second/cover.webp");
            CancellationToken firstToken = default;
            var loader = IRemoteImageLoader.Mock();
            loader.OpenReadAsync(firstUri, Any<CancellationToken>())
                .Callback((_, token) => firstToken = token)
                .ReturnsAsync(firstLoad.Task);
            loader.OpenReadAsync(secondUri, Any<CancellationToken>())
                .Returns(new MemoryStream(MinimalWebp, false));
            AsyncImage.DefaultRemoteLoader = loader;
            var cover = Show(new CoverImage { Source = firstUri.AbsoluteUri });

            cover.Source = secondUri.AbsoluteUri;
            var latest = await WaitForAnimatedSource(cover);
            firstLoad.SetResult(firstStream);
            await WaitUntil(() => !firstStream.CanRead);
            Dispatcher.UIThread.RunJobs();

            using var _ = Assert.Multiple();
            await Assert.That(firstToken.IsCancellationRequested).IsTrue();
            await Assert.That(firstStream.CanRead).IsFalse();
            await Assert.That(latest).IsNotNull();
            await Assert.That(latest!.FrameCount).IsEqualTo(1);
            await Assert.That(ReferenceEquals(AnimatedPart(cover).Source, latest)).IsTrue();
        }
        finally
        {
            firstLoad.TrySetResult(null);
            AsyncImage.DefaultRemoteLoader = previousLoader;
        }
    });

    [Test]
    public Task Source_AnimatedSourceChanged_DisposesPreviousAnimatedSource() => RunOnUI(async () =>
    {
        var pathA = CreateTempWebp(MinimalAnimatedWebp);
        var pathB = CreateTempWebp(MinimalAnimatedWebp);
        try
        {
            var cover = Show(new CoverImage { Source = new Uri(pathA).AbsoluteUri });
            var first = await WaitForAnimatedSource(cover);

            cover.Source = new Uri(pathB).AbsoluteUri;
            var second = await WaitForAnimatedSource(cover, first);
            await WaitUntil(() => first is not null && IsDisposed(first));

            using var _ = Assert.Multiple();
            await Assert.That(first).IsNotNull();
            await Assert.That(second).IsNotNull();
            await Assert.That(IsDisposed(first!)).IsTrue();
        }
        finally
        {
            DeleteTempWebp(pathA);
            DeleteTempWebp(pathB);
        }
    });

    [Test]
    public Task DetachFromVisualTree_LoadedAnimation_DisposesAnimatedSourceAndClearsSource() => RunOnUI(async () =>
    {
        var path = CreateTempWebp(MinimalAnimatedWebp);
        try
        {
            var cover = new CoverImage { Source = new Uri(path).AbsoluteUri };
            var window = new Window { Content = cover, Width = 200, Height = 200 };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var source = await WaitForAnimatedSource(cover);
            var animatedPart = AnimatedPart(cover);

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            await WaitUntil(() => source is not null && IsDisposed(source));

            using var _ = Assert.Multiple();
            await Assert.That(source).IsNotNull();
            await Assert.That(IsDisposed(source!)).IsTrue();
            await Assert.That(animatedPart.Source).IsNull();
        }
        finally
        {
            DeleteTempWebp(path);
        }
    });

    [Test]
    public Task Source_ClearedAfterAnimationLoads_DisposesAnimatedSource() => RunOnUI(async () =>
    {
        var path = CreateTempWebp(MinimalAnimatedWebp);
        try
        {
            var cover = Show(new CoverImage { Source = new Uri(path).AbsoluteUri });
            var source = await WaitForAnimatedSource(cover);

            cover.Source = null;
            Dispatcher.UIThread.RunJobs();
            await WaitUntil(() => source is not null && IsDisposed(source));

            using var _ = Assert.Multiple();
            await Assert.That(source).IsNotNull();
            await Assert.That(IsDisposed(source!)).IsTrue();
            await Assert.That(AnimatedPart(cover).Source).IsNull();
        }
        finally
        {
            DeleteTempWebp(path);
        }
    });

    [Test]
    public Task AttachToVisualTree_AnimatedSourceAfterDetach_RebuildsAnimatedSource() => RunOnUI(async () =>
    {
        var path = CreateTempWebp(MinimalAnimatedWebp);
        try
        {
            var cover = new CoverImage { Source = new Uri(path).AbsoluteUri };
            var window = new Window { Content = cover, Width = 200, Height = 200 };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var first = await WaitForAnimatedSource(cover);

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            window.Content = cover;
            Dispatcher.UIThread.RunJobs();

            var reloaded = await WaitForAnimatedSource(cover);

            using var _ = Assert.Multiple();
            await Assert.That(first).IsNotNull();
            await Assert.That(reloaded).IsNotNull();
            await Assert.That(ReferenceEquals(first, reloaded)).IsFalse();
        }
        finally
        {
            DeleteTempWebp(path);
        }
    });

    [Test]
    public Task Source_BoundInsideItemTemplate_LoadsWebp() => RunOnUI(async () =>
    {
        var path = CreateTempWebp(MinimalAnimatedWebp);
        try
        {
            var items = new ItemsControl
            {
                ItemsSource = new[] { new CoverData { CoverPath = path } },
                ItemTemplate = new FuncDataTemplate<CoverData>((_, _) =>
                {
                    var cover = new CoverImage { Stretch = Stretch.UniformToFill };
                    cover.Bind(CoverImage.SourceProperty,
                        CompiledBinding.Create((CoverData data) => data.CoverPath));
                    return cover;
                })
            };
            var window = new Window { Content = items, Width = 200, Height = 200 };
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var cover = items.GetVisualDescendants().OfType<CoverImage>().First();
            var source = await WaitForAnimatedSource(cover);

            await Assert.That(source).IsNotNull();
            await Assert.That(source!.IsInitialized).IsTrue();
        }
        finally
        {
            DeleteTempWebp(path);
        }
    });

    [Test]
    public Task Source_StaticWebp_DecodesSingleFrame() => RunOnUI(async () =>
    {
        var path = CreateTempWebp(MinimalWebp);
        try
        {
            var cover = Show(new CoverImage { Source = new Uri(path).AbsoluteUri, Stretch = Stretch.UniformToFill });
            var source = await WaitForAnimatedSource(cover);

            using var _ = Assert.Multiple();
            await Assert.That(source).IsNotNull();
            await Assert.That(source!.IsInitialized).IsTrue();
            await Assert.That(source.FrameCount).IsEqualTo(1);
            await Assert.That(AnimatedPart(cover).IsVisible).IsTrue();
        }
        finally
        {
            DeleteTempWebp(path);
        }
    });

    private static CoverImage Show(CoverImage cover)
    {
        var window = new Window { Content = cover, Width = 200, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return cover;
    }

    private static AnimatedImage AnimatedPart(CoverImage cover) =>
        cover.GetVisualDescendants().OfType<AnimatedImage>().First(part => part.Name is "PART_Animated");

    private static async Task<IAnimatedBitmap?> WaitForAnimatedSource(CoverImage cover, IAnimatedBitmap? previous = null)
    {
        var animatedPart = AnimatedPart(cover);
        for (var attempt = 0; attempt < 200; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            if (animatedPart.Source is { } source && !ReferenceEquals(source, previous))
            {
                return source;
            }

            await Task.Delay(5);
        }

        return animatedPart.Source;
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        for (var attempt = 0; attempt < 200; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            if (condition())
            {
                return;
            }

            await Task.Delay(5);
        }
    }

    private static bool IsDisposed(IAnimatedBitmap source)
    {
        foreach (var frame in source.Frames)
        {
            try
            {
                _ = frame.PixelSize;
                return false;
            }
            catch (ObjectDisposedException)
            {
            }
        }

        return true;
    }

    private static string CreateTempWebp(byte[] bytes)
    {
        var directory = Directory.CreateTempSubdirectory("euterpe_coverimage_");
        var path = Path.Combine(directory.FullName, "cover.webp");
        File.WriteAllBytes(path, bytes);
        return path;
    }

    private static void DeleteTempWebp(string path)
    {
        File.Delete(path);
        Directory.Delete(Path.GetDirectoryName(path)!);
    }

    private sealed class CoverData
    {
        public string? CoverPath { get; init; }
    }
}
