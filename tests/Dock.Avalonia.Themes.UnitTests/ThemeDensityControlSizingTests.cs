using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Controls.Overlays;
using Dock.Avalonia.Themes;
using Dock.Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Simple;
using Xunit;

namespace Dock.Avalonia.Themes.UnitTests;

[Collection(ThemeResourceIsolationCollection.Name)]
public class ThemeDensityControlSizingTests
{
    [AvaloniaFact]
    public void CompactDensity_Should_Keep_DocumentTab_MinHeight_And_Update_ToolTab_Metrics_For_All_Themes()
    {
        foreach (var useFluentTheme in new[] { true, false })
        {
            var documentTab = new DocumentTabStripItem();
            var documentWindow = ShowWithTheme(
                documentTab,
                useFluentTheme
                    ? new DockFluentTheme { DensityStyle = DockDensityStyle.Compact }
                    : new DockSimpleTheme { DensityStyle = DockDensityStyle.Compact });
            try
            {
                Assert.Equal(24d, documentTab.MinHeight);
            }
            finally
            {
                documentWindow.Close();
            }

            var toolTab = new ToolTabStripItem();
            var toolWindow = ShowWithTheme(
                toolTab,
                useFluentTheme
                    ? new DockFluentTheme { DensityStyle = DockDensityStyle.Compact }
                    : new DockSimpleTheme { DensityStyle = DockDensityStyle.Compact });
            try
            {
                Assert.Equal(0d, toolTab.MinHeight);
                Assert.Equal(new Thickness(3, 0, 3, 0), toolTab.Padding);
            }
            finally
            {
                toolWindow.Close();
            }
        }
    }

    [AvaloniaFact]
    public void NormalDensity_Should_Keep_Default_Tool_Tab_Metrics_For_All_Themes()
    {
        foreach (var useFluentTheme in new[] { true, false })
        {
            var toolTab = new ToolTabStripItem();
            var toolWindow = ShowWithTheme(
                toolTab,
                useFluentTheme
                    ? new DockFluentTheme { DensityStyle = DockDensityStyle.Normal }
                    : new DockSimpleTheme { DensityStyle = DockDensityStyle.Normal });
            try
            {
                Assert.Equal(0d, toolTab.MinHeight);
                Assert.Equal(new Thickness(4, 1, 4, 0), toolTab.Padding);
            }
            finally
            {
                toolWindow.Close();
            }
        }
    }

    [AvaloniaFact]
    public void Density_Should_Update_Close_And_Chrome_Button_Themes_For_All_Themes()
    {
        AssertThemeButtonSizing(new DockFluentTheme { DensityStyle = DockDensityStyle.Normal }, 14d, 18d, 16d);
        AssertThemeButtonSizing(new DockSimpleTheme { DensityStyle = DockDensityStyle.Normal }, 14d, 18d, 16d);

        AssertThemeButtonSizing(new DockFluentTheme { DensityStyle = DockDensityStyle.Compact }, 12d, 16d, 14d);
        AssertThemeButtonSizing(new DockSimpleTheme { DensityStyle = DockDensityStyle.Compact }, 12d, 16d, 14d);
    }

    [AvaloniaFact]
    public void Theme_Tokens_Should_Bind_To_Key_Control_Template_Parts()
    {
        AssertTemplatePartSizing(new DockFluentTheme { DensityStyle = DockDensityStyle.Normal }, expectedGripHeight: 5d, expectedStatusIconSize: 10d, expectedTargetSelectorSize: 40d, expectedDialogTitleFontSize: 16d, expectedDialogCloseButtonSize: 28d);
        AssertTemplatePartSizing(new DockSimpleTheme { DensityStyle = DockDensityStyle.Normal }, expectedGripHeight: 5d, expectedStatusIconSize: 10d, expectedTargetSelectorSize: 40d, expectedDialogTitleFontSize: 16d, expectedDialogCloseButtonSize: 28d);

        AssertTemplatePartSizing(new DockFluentTheme { DensityStyle = DockDensityStyle.Compact }, expectedGripHeight: 4d, expectedStatusIconSize: 9d, expectedTargetSelectorSize: 40d, expectedDialogTitleFontSize: 14d, expectedDialogCloseButtonSize: 24d);
        AssertTemplatePartSizing(new DockSimpleTheme { DensityStyle = DockDensityStyle.Compact }, expectedGripHeight: 4d, expectedStatusIconSize: 9d, expectedTargetSelectorSize: 40d, expectedDialogTitleFontSize: 14d, expectedDialogCloseButtonSize: 24d);
    }

    private static void AssertThemeButtonSizing(Styles theme, double expectedCloseSize, double expectedChromeWidth, double expectedChromeHeight)
    {
        var host = new StackPanel();
        var closeButton = new Button();
        var chromeButton = new Button();

        host.Children.Add(closeButton);
        host.Children.Add(chromeButton);

        var window = ShowWithTheme(host, theme);
        try
        {
            Assert.True(host.TryFindResource("DocumentCloseButtonTheme", out var closeThemeResource));
            closeButton.Theme = Assert.IsType<ControlTheme>(closeThemeResource);

            Assert.True(host.TryFindResource("ChromeButton", out var chromeThemeResource));
            chromeButton.Theme = Assert.IsType<ControlTheme>(chromeThemeResource);

            closeButton.ApplyTemplate();
            chromeButton.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(expectedCloseSize, closeButton.Width);
            Assert.Equal(expectedCloseSize, closeButton.Height);
            Assert.Equal(expectedChromeWidth, chromeButton.Width);
            Assert.Equal(expectedChromeHeight, chromeButton.Height);
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertTemplatePartSizing(
        Styles theme,
        double expectedGripHeight,
        double expectedStatusIconSize,
        double expectedTargetSelectorSize,
        double expectedDialogTitleFontSize,
        double expectedDialogCloseButtonSize)
    {
        var host = new StackPanel();
        var toolChrome = new ToolChromeControl();
        var dragPreview = new DragPreviewControl
        {
            Status = "Dock"
        };
        var dockTarget = new DockTarget();
        var dialogShell = new DialogShellControl
        {
            Title = "Dialog",
            IsCloseVisible = true
        };

        host.Children.Add(toolChrome);
        host.Children.Add(dragPreview);
        host.Children.Add(dockTarget);
        host.Children.Add(dialogShell);

        var window = ShowWithTheme(host, theme);
        try
        {
            toolChrome.ApplyTemplate();
            dragPreview.ApplyTemplate();
            dockTarget.ApplyTemplate();
            dialogShell.ApplyTemplate();
            toolChrome.UpdateLayout();
            dragPreview.UpdateLayout();
            dockTarget.UpdateLayout();
            dialogShell.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var gripGrid = FindNamedControl<Grid>(toolChrome, "PART_Grid");
            var statusIcon = FindNamedControl<Path>(dragPreview, "PART_StatusIcon");
            var topSelector = FindNamedControl<Image>(dockTarget, "PART_TopSelector");
            var dialogCloseButton = dialogShell.GetVisualDescendants().OfType<Button>().FirstOrDefault();
            var dialogTitle = dialogShell.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault(x => x.Text == "Dialog");
            Assert.NotNull(gripGrid.Background);

            Assert.Equal(expectedGripHeight, gripGrid.Height);
            Assert.True(gripGrid.Bounds.Width >= 12d);
            Assert.Equal(expectedStatusIconSize, statusIcon.Width);
            Assert.Equal(expectedStatusIconSize, statusIcon.Height);
            Assert.Equal(expectedTargetSelectorSize, topSelector.Width);
            Assert.Equal(expectedTargetSelectorSize, topSelector.Height);
            Assert.NotNull(dialogTitle);
            Assert.Equal(expectedDialogTitleFontSize, dialogTitle!.FontSize);
            Assert.NotNull(dialogCloseButton);
            Assert.Equal(expectedDialogCloseButtonSize, dialogCloseButton!.Width);
            Assert.Equal(expectedDialogCloseButtonSize, dialogCloseButton.Height);
        }
        finally
        {
            window.Close();
        }
    }

    private static T FindNamedControl<T>(Control root, string name) where T : Control
    {
        var control = root.GetVisualDescendants().OfType<T>().FirstOrDefault(x => x.Name == name);
        Assert.NotNull(control);
        return control!;
    }

    private static Window ShowWithTheme(Control control, Styles theme)
    {
        var window = new Window
        {
            Width = 640,
            Height = 480,
            Content = control
        };

        window.Styles.Add(theme);
        window.Show();
        window.UpdateLayout();
        control.ApplyTemplate();
        control.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
