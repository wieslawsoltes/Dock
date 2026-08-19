using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Simple;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.Themes.UnitTests;

[Collection(ThemeResourceIsolationCollection.Name)]
public class ToolChromeControlLayoutTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Header_Separates_Long_Title_Grip_And_Buttons(bool useSimpleTheme)
    {
        var factory = new Factory();
        var tool = new Tool { Title = "Tool One With A Long Title That Must Be Trimmed" };
        var toolDock = new ToolDock
        {
            Factory = factory,
            VisibleDockables = new AvaloniaList<IDockable> { tool },
            ActiveDockable = tool
        };
        var chrome = new ToolChromeControl
        {
            Width = 300,
            DataContext = toolDock
        };
        var window = new Window
        {
            Width = 300,
            Height = 200,
            Content = chrome
        };
        Styles theme = useSimpleTheme ? new DockSimpleTheme() : new DockFluentTheme();
        window.Styles.Add(theme);
        window.Resources["DockChromeGripBrush"] = Brushes.Red;

        window.Show();
        try
        {
            window.UpdateLayout();
            chrome.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var title = FindNamedControl<TextBlock>(chrome, "PART_Title");
            var grip = FindNamedControl<Grid>(chrome, "PART_Grid");
            var header = Assert.IsType<Grid>(grip.Parent);
            var titleHost = Assert.IsAssignableFrom<Panel>(title.Parent);
            var buttonStrip = Assert.Single(header.Children.OfType<StackPanel>());
            var gripBrush = Assert.IsAssignableFrom<ISolidColorBrush>(grip.Background);

            Assert.Equal(Colors.Red, gripBrush.Color);
            Assert.Equal(0, Grid.GetColumn(titleHost));
            Assert.Equal(1, Grid.GetColumn(grip));
            Assert.Equal(2, Grid.GetColumn(buttonStrip));
            Assert.True(
                titleHost.Bounds.Right <= grip.Bounds.Left,
                $"Title host {titleHost.Bounds} overlaps grip {grip.Bounds} in header {header.Bounds}.");
            Assert.True(
                grip.Bounds.Right <= buttonStrip.Bounds.Left,
                $"Grip {grip.Bounds} overlaps button strip {buttonStrip.Bounds} in header {header.Bounds}.");
            Assert.True(
                buttonStrip.Bounds.Right <= header.Bounds.Width,
                $"Button strip {buttonStrip.Bounds} exceeds header {header.Bounds}.");
            Assert.True(grip.Bounds.Width >= 12d);
            Assert.Equal(TextTrimming.CharacterEllipsis, title.TextTrimming);
        }
        finally
        {
            window.Close();
        }
    }

    private static T FindNamedControl<T>(Control root, string name) where T : Control
    {
        T? control = root.GetVisualDescendants().OfType<T>().FirstOrDefault(x => x.Name == name);
        return Assert.IsType<T>(control);
    }
}
