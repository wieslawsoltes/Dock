using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class InheritedControlPropertyTests
{
    [AvaloniaTheory]
    [InlineData("document")]
    [InlineData("tool")]
    [InlineData("group")]
    public void Floating_Hosts_Are_Excluded_From_The_Taskbar_Before_Styling(string kind)
    {
        LayoutFloatingWindowControl host = CreateHost(kind);
        try
        {
            Assert.False(host.ShowInTaskbar);
            host.ShowInTaskbar = true;
            host.ClearValue(Window.ShowInTaskbarProperty);
            Assert.False(host.ShowInTaskbar);
        }
        finally { host.Close(); }
    }

    [AvaloniaTheory]
    [InlineData("document")]
    [InlineData("tool")]
    [InlineData("group")]
    public void Floating_Taskbar_Styles_Can_Be_Replaced_And_Released(string kind)
    {
        LayoutFloatingWindowControl host = CreateHost(kind);
        try
        {
            host.Theme = new ControlTheme(typeof(LayoutFloatingWindowControl));
            Style style = new() { Setters = { new Setter(Window.ShowInTaskbarProperty, true) } };
            host.Styles.Add(style);
            host.ApplyStyling();
            Assert.True(host.ShowInTaskbar);
            host.ShowInTaskbar = false;
            Assert.False(host.ShowInTaskbar);
            host.ClearValue(Window.ShowInTaskbarProperty);
            Assert.True(host.ShowInTaskbar);
            host.Styles.Remove(style);
            Assert.False(host.ShowInTaskbar);
        }
        finally { host.Close(); }
    }

    [AvaloniaFact]
    public void An_Unstyled_Resizer_Has_A_Transparent_Hit_Surface()
    {
        LayoutGridResizerControl splitter = new();
        Assert.Same(Brushes.Transparent, splitter.Background);
        splitter.Background = Brushes.Red;
        splitter.ClearValue(TemplatedControl.BackgroundProperty);
        Assert.Same(Brushes.Transparent, splitter.Background);
    }

    [AvaloniaFact]
    public void A_Replacement_Resizer_Theme_Can_Release_Its_Background()
    {
        LayoutGridResizerControl splitter = new()
        {
            Theme = new ControlTheme(typeof(LayoutGridResizerControl))
        };
        Style style = new() { Setters = { new Setter(TemplatedControl.BackgroundProperty, Brushes.Red) } };
        splitter.Styles.Add(style);
        splitter.ApplyStyling();
        Assert.Same(Brushes.Red, splitter.Background);
        splitter.Styles.Remove(style);
        Assert.Same(Brushes.Transparent, splitter.Background);
    }

    [AvaloniaFact]
    public void AutoHide_Focus_And_Visibility_Are_Inherited_Defaults()
    {
        DockingManager manager = new();
        manager.OnApplyTemplate();
        LayoutAutoHideWindowControl preview = Assert.IsType<LayoutAutoHideWindowControl>(manager.AutoHideWindow);
        Assert.True(preview.Focusable);
        Assert.True(preview.IsTabStop);
        Assert.False(preview.IsVisible);
        preview.Focusable = false;
        preview.ClearValue(LayoutAutoHideWindowControl.FocusableProperty);
        preview.ClearValue(LayoutAutoHideWindowControl.IsVisibleProperty);
        Assert.True(preview.Focusable);
        Assert.False(preview.IsVisible);
    }

    [AvaloniaFact]
    public void AutoHide_Focus_Styles_Override_And_Release_The_Default()
    {
        DockingManager manager = new();
        manager.OnApplyTemplate();
        LayoutAutoHideWindowControl preview = Assert.IsType<LayoutAutoHideWindowControl>(manager.AutoHideWindow);
        Style style = new() { Setters = { new Setter(LayoutAutoHideWindowControl.FocusableProperty, false) } };
        preview.Styles.Add(style);
        preview.ApplyStyling();
        Assert.False(preview.Focusable);
        preview.Focusable = true;
        preview.ClearValue(LayoutAutoHideWindowControl.FocusableProperty);
        Assert.False(preview.Focusable);
        preview.Styles.Remove(style);
        Assert.True(preview.Focusable);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Overlay_Taskbar_And_Activation_Defaults_Survive_Theme_Replacement(bool activation)
    {
        OverlayHost overlay = new();
        try
        {
            StyledProperty<bool> property = activation ? Window.ShowActivatedProperty : Window.ShowInTaskbarProperty;
            Assert.False(overlay.GetValue(property));
            overlay.Theme = new ControlTheme(typeof(OverlayWindow));
            Style style = new() { Setters = { new Setter(property, true) } };
            overlay.Styles.Add(style);
            overlay.ApplyStyling();
            Assert.True(overlay.GetValue(property));
            overlay.Styles.Remove(style);
            Assert.False(overlay.GetValue(property));
        }
        finally { overlay.Close(); }
    }

    [AvaloniaFact]
    public void Overlay_Decorations_Default_To_None_And_Remain_Styleable()
    {
        OverlayHost overlay = new();
        try
        {
            Assert.Equal(WindowDecorations.None, overlay.WindowDecorations);
            overlay.Theme = new ControlTheme(typeof(OverlayWindow))
            { Setters = { new Setter(Window.WindowDecorationsProperty, WindowDecorations.Full) } };
            overlay.ApplyStyling();
            Assert.Equal(WindowDecorations.Full, overlay.WindowDecorations);
            overlay.Theme = new ControlTheme(typeof(OverlayWindow));
            overlay.ApplyStyling();
            Assert.Equal(WindowDecorations.None, overlay.WindowDecorations);
        }
        finally { overlay.Close(); }
    }

    [AvaloniaFact]
    public void Overlay_Transparency_Default_Remains_Available_To_Custom_Themes()
    {
        OverlayHost overlay = new();
        try
        {
            Assert.Equal(WindowTransparencyLevel.Transparent, Assert.Single(overlay.TransparencyLevelHint));
            overlay.TransparencyLevelHint = new[] { WindowTransparencyLevel.None };
            overlay.ClearValue(TopLevel.TransparencyLevelHintProperty);
            Assert.Equal(WindowTransparencyLevel.Transparent, Assert.Single(overlay.TransparencyLevelHint));
            Assert.False(overlay.IsVisible);
        }
        finally { overlay.Close(); }
    }

    [AvaloniaFact]
    public void Compatibility_Defaults_Do_Not_Change_Native_Or_Framework_Controls()
    {
        HostWindow native = new();
        try
        {
            Assert.True(native.ShowInTaskbar);
            Assert.True(native.ShowActivated);
            Assert.Equal(WindowDecorations.Full, native.WindowDecorations);
            Assert.Empty(native.TransparencyLevelHint);
            Assert.Null(new Thumb().Background);
            ContentControl content = new();
            Assert.False(content.Focusable);
            Assert.True(content.IsVisible);
        }
        finally { native.Close(); }
    }

    private static LayoutFloatingWindowControl CreateHost(string kind) => kind switch
    {
        "document" => new DocumentHost(),
        "tool" => new ToolHost(),
        _ => new GroupHost()
    };

    private sealed class DocumentHost() : LayoutDocumentFloatingWindowControl(new LayoutDocumentFloatingWindow());
    private sealed class ToolHost() : LayoutAnchorableFloatingWindowControl(new LayoutAnchorableFloatingWindow());
    private sealed class GroupHost() : LayoutGroupFloatingWindowControl(new LayoutGroupFloatingWindow());
    private sealed class OverlayHost() : OverlayWindow(new DockingManager(), new Panel(), new Panel());
}
