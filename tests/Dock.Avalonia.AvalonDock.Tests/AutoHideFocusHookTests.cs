using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock.Controls;
using AvalonDock.Themes;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class AutoHideFocusHookTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void The_Base_Hook_Remains_False_While_Avalonia_Tracks_Actual_Preview_Focus(bool focusContent)
    {
        Button editor = new() { Content = "Preview content" };
        LayoutAutoHideWindowControl preview = CreatePreview();
        preview.Content = editor;
        preview.IsVisible = true;
        Button outside = new() { Content = "Outside preview" };
        Window window = new()
        {
            Content = new StackPanel { Children = { outside, preview } },
            Width = 300,
            Height = 200
        };
        window.Styles.Add(new AvalonDockTheme());
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Assert.True(outside.Focus());
            Assert.Same(outside, window.FocusManager!.GetFocusedElement());
            Assert.False(preview.IsKeyboardFocusWithin);
            Assert.False(ReadBaseFocus(preview));

            Control focused = focusContent ? editor : preview;
            Assert.True(focused.Focus());
            Assert.Same(focused, window.FocusManager.GetFocusedElement());
            Assert.True(preview.IsKeyboardFocusWithin);
            Assert.False(ReadBaseFocus(preview));
            Assert.True(preview.IsKeyboardFocusWithin);

            Assert.True(outside.Focus());
            Assert.False(preview.IsKeyboardFocusWithin);
            Assert.False(ReadBaseFocus(preview));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    // The upstream-compatible internal constructor prevents a consumer subclass.
    // Access the protected hook without reflection or widening the production API.
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "HasFocusWithinCore")]
    private static extern bool ReadBaseFocus(LayoutAutoHideWindowControl preview);

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern LayoutAutoHideWindowControl CreatePreview();
}
