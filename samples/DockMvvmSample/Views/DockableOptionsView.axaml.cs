using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Model.Core;

namespace DockMvvmSample.Views;

public partial class DockableOptionsView : UserControl
{
    public static object?[] PinnedDockDisplayModeOptions { get; } =
    {
        null,
        PinnedDockDisplayMode.Overlay,
        PinnedDockDisplayMode.Inline
    };

    public static DockingWindowState[] DockingWindowStateOptions { get; } =
    {
        DockingWindowState.Docked,
        DockingWindowState.Pinned,
        DockingWindowState.Document,
        DockingWindowState.Docked | DockingWindowState.Floating,
        DockingWindowState.Pinned | DockingWindowState.Floating,
        DockingWindowState.Document | DockingWindowState.Floating,
        DockingWindowState.Docked | DockingWindowState.Hidden,
        DockingWindowState.Pinned | DockingWindowState.Hidden,
        DockingWindowState.Document | DockingWindowState.Hidden,
        DockingWindowState.Docked | DockingWindowState.Floating | DockingWindowState.Hidden,
        DockingWindowState.Document | DockingWindowState.Floating | DockingWindowState.Hidden
    };

    public DockableOptionsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
