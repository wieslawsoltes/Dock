using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Dock.Avalonia.Internal;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class ItemDragHelperTests
{
    [AvaloniaFact]
    public void PointerMoved_Refreshes_Invalid_Bounds_Before_DragOutside_Check()
    {
        var listBox = new ListBox
        {
            Width = 400,
            Height = 32,
            ItemsSource = new[] { "One", "Two" }
        };

        var window = new Window
        {
            Width = 400,
            Height = 100,
            Content = listBox
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var item = Assert.IsType<ListBoxItem>(listBox.ContainerFromIndex(0));
            var draggedOutside = false;
            var helper = new ItemDragHelper(
                item,
                () => listBox,
                () => Orientation.Horizontal,
                dragOutside: (_, _) => draggedOutside = true,
                getBoundsContainer: () => listBox);

            helper.Attach();
            try
            {
                var pointer = new Pointer(7, PointerType.Mouse, true);
                item.RaiseEvent(CreatePressedArgs(item, listBox, new Point(5, 5), pointer));

                listBox.Width = 40;
                listBox.InvalidateMeasure();
                listBox.InvalidateArrange();

                item.RaiseEvent(CreateMovedArgs(item, listBox, new Point(100, 5), pointer));

                Assert.True(draggedOutside);
            }
            finally
            {
                helper.Detach();
            }
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void PointerMoved_Outside_ScrollViewport_Starts_DragOutside_Even_When_Inside_Outer_Bounds()
    {
        var listBox = new ListBox
        {
            Width = 40,
            Height = 32,
            ItemsSource = new[] { "One", "Two", "Three", "Four", "Five", "Six" }
        };

        var boundsContainer = new Grid
        {
            Width = 400,
            Height = 32,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Children =
            {
                listBox
            }
        };

        var window = new Window
        {
            Width = 400,
            Height = 100,
            Content = boundsContainer
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var item = Assert.IsType<ListBoxItem>(listBox.ContainerFromIndex(0));
            var draggedOutside = false;
            var helper = new ItemDragHelper(
                item,
                () => listBox,
                () => Orientation.Horizontal,
                dragOutside: (_, _) => draggedOutside = true,
                getBoundsContainer: () => boundsContainer);

            helper.Attach();
            try
            {
                var pointer = new Pointer(8, PointerType.Mouse, true);
                item.RaiseEvent(CreatePressedArgs(item, listBox, new Point(5, 5), pointer));
                item.RaiseEvent(CreateMovedArgs(item, listBox, new Point(100, 5), pointer));

                Assert.True(draggedOutside);
            }
            finally
            {
                helper.Detach();
            }
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
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
