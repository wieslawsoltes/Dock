using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DocumentTabStripSelectionRestoreTests
{
    [AvaloniaFact]
    public void DragOutside_RestoresSelection()
    {
        var (window, tabStrip) = CreateTabStrip();
        try
        {
            tabStrip.SelectedIndex = 0;
            var originalIndex = tabStrip.SelectedIndex;

            var tabItem = GetTabItem(tabStrip, 1);

            var pointer = new Pointer(1, PointerType.Mouse, true);
            var pressed = CreatePressedArgs(tabItem, tabStrip, new Point(5, 5), pointer);
            tabItem.RaiseEvent(pressed);

            tabStrip.SelectedIndex = 1;
            Assert.Equal(1, tabStrip.SelectedIndex);

            var moved = CreateMovedArgs(tabItem, tabStrip, new Point(-50, 5), pointer);
            tabItem.RaiseEvent(moved);

            Assert.Equal(originalIndex, tabStrip.SelectedIndex);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DragThreshold_RestoresSelection()
    {
        var (window, tabStrip) = CreateTabStrip();
        try
        {
            tabStrip.SelectedIndex = 0;
            var originalIndex = tabStrip.SelectedIndex;

            var tabItem = GetTabItem(tabStrip, 1);

            var pointer = new Pointer(2, PointerType.Mouse, true);
            var pressed = CreatePressedArgs(tabItem, tabStrip, new Point(5, 5), pointer);
            tabItem.RaiseEvent(pressed);

            tabStrip.SelectedIndex = 1;
            Assert.Equal(1, tabStrip.SelectedIndex);

            var moved = CreateMovedArgs(tabItem, tabStrip, new Point(20, 5), pointer);
            tabItem.RaiseEvent(moved);

            Assert.Equal(originalIndex, tabStrip.SelectedIndex);
        }
        finally
        {
            window.Close();
        }
    }

    private static (Window window, DocumentTabStrip tabStrip) CreateTabStrip()
    {
        var tabStrip = new DocumentTabStrip
        {
            Width = 400,
            Height = 32,
            ItemsSource = new AvaloniaList<string> { "Doc1", "Doc2" }
        };

        var window = new Window
        {
            Width = 400,
            Height = 200,
            Content = tabStrip
        };

        window.Show();
        tabStrip.ApplyTemplate();
        window.UpdateLayout();
        tabStrip.UpdateLayout();

        return (window, tabStrip);
    }

    private static DocumentTabStripItem GetTabItem(DocumentTabStrip tabStrip, int index)
    {
        var tabItem = tabStrip.ContainerFromIndex(index) as DocumentTabStripItem;
        Assert.NotNull(tabItem);
        return tabItem!;
    }

    private static PointerPressedEventArgs CreatePressedArgs(Control source, Visual rootVisual, Point position, Pointer pointer)
    {
        var properties = new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
        return new PointerPressedEventArgs(source, pointer, rootVisual, position, 0, properties, KeyModifiers.None, 1);
    }

    private static PointerEventArgs CreateMovedArgs(Control source, Visual rootVisual, Point position, Pointer pointer)
    {
        var properties = new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.Other);
        return new PointerEventArgs(InputElement.PointerMovedEvent, source, pointer, rootVisual, position, 0, properties, KeyModifiers.None);
    }
}
