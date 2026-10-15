using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Embedding;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Platform;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.LogicalTree;
using Avalonia.Platform;
using Avalonia.Platform.Surfaces;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Controls;
using AvalonDock.Themes;
using Dock.Avalonia.Controls;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

// Recording platform services exercise the real NativeControlHost lifecycle without an OS window.
public class AutoHideNativeHostTests
{
    [AvaloniaFact]
    public void Native_Child_Uses_Existing_Editor_And_Preserves_Logical_Input_And_Resource_Scopes()
    {
        using IDisposable scope = AvaloniaLocator.EnterScope();
        RecordingPlatform platform = new(AvaloniaLocator.Current.GetRequiredService<IWindowingPlatform>());
        AvaloniaLocator.CurrentMutable.Bind<IWindowingPlatform>().ToConstant(platform);
        LayoutAutoHideWindowControl preview = CreatePreview();
        Button editor = new() { Content = "Existing editor" };
        preview.Content = editor;
        preview.IsVisible = true;
        preview.Width = 250;
        preview.Height = 150;
        preview.Resources["preview resource"] = "value";
        RecordingTopLevel ownerImpl = platform.CreateOwner();
        using EmbeddableControlRoot owner = new(ownerImpl) { Content = preview };
        owner.Styles.Add(new AvalonDockTheme());
        owner.Prepare();
        Dispatcher.UIThread.RunJobs();

        RecordingTopLevel embeddedImpl = Assert.Single(platform.Embedded);
        EmbeddableControlRoot embedded = Assert.IsAssignableFrom<EmbeddableControlRoot>(TopLevel.GetTopLevel(editor));
        Assert.NotSame(owner, embedded);
        Assert.Same(preview.Presenter, embedded.Content);
        Assert.Same(editor, preview.Presenter!.Child);
        Assert.Same(preview.Presenter, editor.Parent);
        Assert.Same(preview, preview.Presenter.Parent);
        Assert.Equal(new[] { preview.Presenter }, preview.GetLogicalChildren());
        Assert.Equal("value", editor.FindResource("preview resource"));
        Assert.Equal(embeddedImpl.Handle, Assert.Single(platform.Native.Attachments).Child);
        Assert.True(editor.Focus());
        Assert.True(preview.IsKeyboardFocusWithin);
        bool routed = false;
        preview.KeyDown += (_, _) => routed = true;
        editor.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.A });
        Assert.True(routed);
        Assert.Equal(0, embeddedImpl.Disposals);
        Assert.True(platform.Native.Attachments[0].Shown.Width > 0);
    }

    [AvaloniaFact]
    public void Detach_Destroys_Child_Once_And_Reattach_Reuses_The_Editor_In_A_New_Child()
    {
        using IDisposable scope = AvaloniaLocator.EnterScope();
        RecordingPlatform platform = new(AvaloniaLocator.Current.GetRequiredService<IWindowingPlatform>());
        AvaloniaLocator.CurrentMutable.Bind<IWindowingPlatform>().ToConstant(platform);
        LayoutAutoHideWindowControl preview = CreatePreview();
        Button editor = new() { Content = "Existing editor" };
        preview.Content = editor;
        preview.IsVisible = true;
        using EmbeddableControlRoot owner = new(platform.CreateOwner()) { Content = preview };
        owner.Styles.Add(new AvalonDockTheme());
        owner.Prepare();
        Dispatcher.UIThread.RunJobs();
        RecordingTopLevel first = Assert.Single(platform.Embedded);

        owner.Content = null;
        owner.Presenter!.UpdateChild();
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(1, first.Disposals);
        Assert.Equal(1, platform.Native.Attachments[0].Disposals);
        Assert.Same(editor, preview.Content);
        owner.Content = preview;
        owner.Presenter.UpdateChild();
        owner.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        Assert.True(platform.Embedded.Count >= 2);
        Assert.All(platform.Embedded.SkipLast(1), implementation => Assert.Equal(1, implementation.Disposals));
        Assert.Equal(0, platform.Embedded[^1].Disposals);
        Assert.Equal(platform.Embedded[^1].Handle!.Handle, TopLevel.GetTopLevel(editor)!.TryGetPlatformHandle()!.Handle);
        Assert.All(platform.Native.Attachments.SkipLast(1), attachment => Assert.Equal(1, attachment.Disposals));
        Assert.Equal(0, platform.Native.Attachments[^1].Disposals);
        Assert.Same(editor, preview.Presenter!.Child);
        Assert.Equal(1, first.Disposals);
    }

    [AvaloniaFact]
    public void Backend_Without_Embedding_Retains_The_Same_InTree_Editor()
    {
        LayoutAutoHideWindowControl preview = CreatePreview();
        Button editor = new() { Content = "Existing editor" };
        preview.Content = editor;
        preview.IsVisible = true;
        Window owner = new() { Content = preview, Width = 300, Height = 200 };
        owner.Styles.Add(new AvalonDockTheme());
        try
        {
            owner.Show();
            Dispatcher.UIThread.RunJobs();
            owner.UpdateLayout();
            Assert.Same(owner, TopLevel.GetTopLevel(editor));
            Assert.Same(editor, preview.Presenter!.Child);
            Assert.Same(preview.Presenter, editor.Parent);
            Assert.Equal(new[] { preview.Presenter }, preview.GetLogicalChildren());
            Assert.True(editor.Focus());
            Assert.True(preview.IsKeyboardFocusWithin);
        }
        finally { owner.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Resize_Ghost_Uses_A_Separate_Popup_And_Closes_On_Cancel_Or_Commit(bool cancel)
    {
        using PinnedWindowSetting nativeWindowDefault = new();
        using IDisposable scope = AvaloniaLocator.EnterScope();
        RecordingPlatform platform = new(AvaloniaLocator.Current.GetRequiredService<IWindowingPlatform>());
        AvaloniaLocator.CurrentMutable.Bind<IWindowingPlatform>().ToConstant(platform);
        LayoutAnchorable tool = new() { Title = "Editor", Content = new Button(), AutoHideWidth = 150 };
        LayoutAnchorGroup group = new(); group.Children.Add(tool);
        LayoutRoot layout = new(); layout.LeftSide!.Children.Add(group);
        DockingManager manager = new() { Layout = layout, AutoHideWindowClosingTimer = int.MaxValue };
        using EmbeddableControlRoot owner = new(platform.CreateOwner());
        owner.Styles.Add(new AvalonDockTheme());
        owner.Content = manager;
        owner.Prepare();
        Update();
        tool.IsSelected = true;
        Update();
        Assert.Same(tool, manager.AutoHideWindow?.Model);
        Assert.NotNull(manager.AutoHideWindow?.Content);
        Assert.Same(owner, TopLevel.GetTopLevel(manager.AutoHideWindow!));
        Assert.NotEmpty(platform.Embedded);
        PinnedDockControl[] pinnedHosts = manager.GetVisualDescendants().OfType<PinnedDockControl>().ToArray();
        Assert.NotEmpty(pinnedHosts);
        Assert.All(pinnedHosts, host => Assert.False(host.UsePinnedDockWindow));
        LayoutGridResizerControl splitter = manager.GetVisualDescendants().OfType<LayoutGridResizerControl>()
            .Single(control => control.Name == "PART_PinnedDockSplitter" && control.IsEffectivelyVisible);
        Popup popup = manager.GetVisualDescendants().OfType<Popup>().Single(control => control.Name == "PART_NativeResizePreview");
        Border inline = manager.GetVisualDescendants().OfType<Border>().Single(control => control.Name == "PART_PinnedDockResizePreview");
        splitter.RaiseEvent(new VectorEventArgs { RoutedEvent = Thumb.DragStartedEvent });
        Update();
        Assert.True(popup.IsOpen);
        Assert.False(inline.IsVisible);
        Assert.IsType<PopupRoot>(TopLevel.GetTopLevel(popup.Child!));
        Assert.Equal(inline.Width, popup.Child!.Bounds.Width);
        Rect before = popup.PlacementRect!.Value;
        splitter.RaiseEvent(new VectorEventArgs { RoutedEvent = Thumb.DragDeltaEvent, Vector = new Vector(35, 0) });
        Update();
        Assert.Equal(before.X + 35, popup.PlacementRect!.Value.X);
        Assert.Equal(150, tool.AutoHideWidth);
        if (cancel)
            owner.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Escape });
        else
            splitter.RaiseEvent(new VectorEventArgs { RoutedEvent = Thumb.DragCompletedEvent, Vector = new Vector(35, 0) });
        Update();
        Assert.False(popup.IsOpen);
        Assert.False(inline.IsVisible);
        Assert.Equal(cancel ? 150 : 185, tool.AutoHideWidth);

        void Update() { Dispatcher.UIThread.RunJobs(); owner.UpdateLayout(); }
    }

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern LayoutAutoHideWindowControl CreatePreview();

    private sealed class PinnedWindowSetting : IDisposable
    {
        private readonly bool _previous = DockSettings.UsePinnedDockWindow;
        internal PinnedWindowSetting() => DockSettings.UsePinnedDockWindow = true;
        public void Dispose() => DockSettings.UsePinnedDockWindow = _previous;
    }

    private sealed class RecordingPlatform(IWindowingPlatform inner) : IWindowingPlatform
    {
        private int _nextHandle = 100;
        internal List<RecordingTopLevel> Embedded { get; } = new();
        internal RecordingNativeHost Native { get; } = new();
        internal RecordingTopLevel CreateOwner() => new(inner.CreateWindow(), new IntPtr(++_nextHandle), Native,
            // The recording native backend supports independent popup surfaces. Headless
            // normally returns null to force overlays, but its window also implements IPopupImpl.
            () => (IPopupImpl)inner.CreateWindow());
        public IWindowImpl CreateWindow() => inner.CreateWindow();
        public IWindowImpl CreateEmbeddableWindow() => throw new NotSupportedException();
        public ITopLevelImpl CreateEmbeddableTopLevel()
        {
            RecordingTopLevel result = new(inner.CreateWindow(), new IntPtr(++_nextHandle));
            Embedded.Add(result);
            Native.Children.Add(result.Handle!.Handle, result);
            return result;
        }
        public ITrayIconImpl? CreateTrayIcon() => inner.CreateTrayIcon();
        public void GetWindowsZOrder(ReadOnlySpan<IWindowImpl> windows, Span<long> zOrder) => inner.GetWindowsZOrder(windows, zOrder);
    }

    private sealed class RecordingNativeHost : INativeControlHostImpl
    {
        internal List<Attachment> Attachments { get; } = new();
        internal Dictionary<IntPtr, RecordingTopLevel> Children { get; } = new();
        public INativeControlHostDestroyableControlHandle CreateDefaultChild(IPlatformHandle parent) => throw new InvalidOperationException("The adapter must supply its embedded root.");
        public bool IsCompatibleWith(IPlatformHandle handle) => handle.HandleDescriptor == "HWND";
        public INativeControlHostControlTopLevelAttachment CreateNewAttachment(Func<IPlatformHandle, IPlatformHandle> create) =>
            CreateNewAttachment(create(new PlatformHandle(new IntPtr(999), "HWND")));
        public INativeControlHostControlTopLevelAttachment CreateNewAttachment(IPlatformHandle handle)
        {
            Attachment result = new(this, handle);
            Attachments.Add(result);
            return result;
        }
        internal sealed class Attachment(RecordingNativeHost owner, IPlatformHandle child) : INativeControlHostControlTopLevelAttachment
        {
            public INativeControlHostImpl? AttachedTo { get; set; } = owner;
            internal IPlatformHandle Child { get; } = child;
            internal Rect Shown { get; private set; }
            internal int Disposals { get; private set; }
            public bool IsCompatibleWith(INativeControlHostImpl host) => ReferenceEquals(owner, host);
            public void ShowInBounds(Rect rect) { Shown = rect; owner.Children[Child.Handle].Resize(rect.Size); }
            public void HideWithSize(Size size) => owner.Children[Child.Handle].Resize(size);
            public void Dispose() { ++Disposals; AttachedTo = null; }
        }
    }

    private sealed class RecordingTopLevel(ITopLevelImpl inner, IntPtr handle, INativeControlHostImpl? native = null, Func<IPopupImpl>? createPopup = null) : ITopLevelImpl
    {
        private Size _size = new(400, 300);
        internal int Disposals { get; private set; }
        internal void Resize(Size size) { _size = size; Resized?.Invoke(size, WindowResizeReason.Unspecified); }
        public double DesktopScaling => inner.DesktopScaling;
        public IPlatformHandle? Handle { get; } = new PlatformHandle(handle, "HWND");
        public Size ClientSize => _size;
        public double RenderScaling => inner.RenderScaling;
        public IPlatformRenderSurface[] Surfaces => inner.Surfaces;
        public Action<RawInputEventArgs>? Input { get => inner.Input; set => inner.Input = value; }
        public Action<Rect>? Paint { get => inner.Paint; set => inner.Paint = value; }
        public Action<Size, WindowResizeReason>? Resized { get => inner.Resized; set => inner.Resized = value; }
        public Action<double>? ScalingChanged { get => inner.ScalingChanged; set => inner.ScalingChanged = value; }
        public Action<WindowTransparencyLevel>? TransparencyLevelChanged { get => inner.TransparencyLevelChanged; set => inner.TransparencyLevelChanged = value; }
        public Compositor Compositor => inner.Compositor;
        public void SetInputRoot(IInputRoot inputRoot) => inner.SetInputRoot(inputRoot);
        public Point PointToClient(PixelPoint point) => inner.PointToClient(point);
        public PixelPoint PointToScreen(Point point) => inner.PointToScreen(point);
        public void SetCursor(ICursorImpl? cursor) => inner.SetCursor(cursor);
        public Action? Closed { get => inner.Closed; set => inner.Closed = value; }
        public Action? LostFocus { get => inner.LostFocus; set => inner.LostFocus = value; }
        public IPopupImpl? CreatePopup() => createPopup?.Invoke() ?? inner.CreatePopup();
        public void SetTransparencyLevelHint(IReadOnlyList<WindowTransparencyLevel> levels) => inner.SetTransparencyLevelHint(levels);
        public WindowTransparencyLevel TransparencyLevel => inner.TransparencyLevel;
        public AcrylicPlatformCompensationLevels AcrylicCompensationLevels => inner.AcrylicCompensationLevels;
        public void SetFrameThemeVariant(PlatformThemeVariant? variant) => inner.SetFrameThemeVariant(variant);
        public object? TryGetFeature(Type featureType) => featureType == typeof(INativeControlHostImpl) ? native : inner.TryGetFeature(featureType);
        public void Dispose() { ++Disposals; inner.Dispose(); }
    }
}
