using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using ReactiveUI.Reactive;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class TabInputContractTests
{
    public static IEnumerable<object[]> ActivationCases()
    {
        foreach (string kind in new[] { "document", "tool", "title" })
        foreach (string condition in new[] { "plain", "handled", "disabled-control", "disabled-model", "interactive", "foreign" })
            yield return new object[] { kind, condition };
    }

    [AvaloniaTheory]
    [MemberData(nameof(ActivationCases))]
    public void Explicit_Tab_And_Title_Activation_Hooks_Activate_Their_Model_Without_Gesture_Filtering(string kind, string condition)
    {
        LayoutContent model = kind == "document"
            ? new LayoutDocument { Content = new object(), CanMove = false }
            : new LayoutAnchorable { Content = new object() };
        ILayoutPanelElement pane = kind == "document"
            ? new LayoutDocumentPane((LayoutDocument)model)
            : new LayoutAnchorablePane((LayoutAnchorable)model);
        DockingManager manager = new() { Layout = new() { RootPanel = new(pane) } };
        TemplatedControl control = kind switch
        {
            "document" => new DocumentProbe { Model = model },
            "tool" => new ToolProbe { Model = model },
            _ => new TitleProbe { Model = (LayoutAnchorable)model }
        };
        control.IsEnabled = condition != "disabled-control";
        model.IsEnabled = condition != "disabled-model";
        manager.Layout.ActiveContent = null;
        Control source = condition switch
        {
            "interactive" => new Button(),
            "foreign" => new DockingManager(),
            _ => control
        };
        Pointer pointer = new(1, PointerType.Mouse, true);
        bool handled = condition == "handled";

        if (control is DocumentProbe document)
        {
            PointerPressedEventArgs args = Press(source, pointer, MouseButton.Left, handled);
            document.PressHook(args);
            Assert.Equal(handled, args.Handled);
        }
        else
        {
            PointerReleasedEventArgs args = new(source, pointer, source, default, 0,
                new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased),
                KeyModifiers.None, MouseButton.Left) { Handled = handled };
            if (control is ToolProbe tool) tool.ReleaseHook(args);
            else ((TitleProbe)control).ReleaseHook(args);
            Assert.Equal(handled, args.Handled);
        }

        Assert.True(model.IsActive);
        Assert.Same(model, manager.Layout.ActiveContent);
        Assert.Empty(manager.Layout.FloatingWindows);
    }

    [AvaloniaTheory]
    [InlineData(false, MouseButton.Middle)]
    [InlineData(true, MouseButton.Middle)]
    [InlineData(false, MouseButton.Left)]
    [InlineData(true, MouseButton.Left)]
    public void Document_MouseDown_Hook_Uses_The_Current_Item_Command_Without_Requiring_A_Model_Or_Changing_Handled(
        bool handled, MouseButton button)
    {
        int calls = 0;
        using BehaviorSubject<bool> enabled = new(true);
        using var command = ReactiveCommand.Create<object?>(parameter =>
        {
            Assert.Null(parameter);
            calls++;
        }, enabled, ImmediateScheduler.Instance);
        DocumentProbe control = new() { IsEnabled = false };
        control.AssignItem(new LayoutDocumentItem { CloseCommand = command });
        Pointer pointer = new(1, PointerType.Mouse, true);
        PointerPressedEventArgs args = Press(control, pointer, button, handled);

        control.MouseDownHook(args);
        Assert.Equal(button == MouseButton.Middle ? 1 : 0, calls);
        Assert.Equal(handled, args.Handled);
        enabled.OnNext(false);
        Dispatcher.UIThread.RunJobs();
        control.MouseDownHook(args);
        Assert.Equal(button == MouseButton.Middle ? 1 : 0, calls);
        Assert.Equal(handled, args.Handled);
    }

    private static PointerPressedEventArgs Press(Control source, Pointer pointer, MouseButton button, bool handled) =>
        new(source, pointer, source, default, 0,
            new PointerPointProperties(button == MouseButton.Middle ? RawInputModifiers.MiddleMouseButton : RawInputModifiers.LeftMouseButton,
                button == MouseButton.Middle ? PointerUpdateKind.MiddleButtonPressed : PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None, 1) { Handled = handled };

    private sealed class DocumentProbe : LayoutDocumentTabItem
    {
        internal void PressHook(PointerPressedEventArgs args) => base.OnMouseLeftButtonDown(args);
        internal void MouseDownHook(PointerPressedEventArgs args) => base.OnMouseDown(args);
        internal void AssignItem(LayoutItem item) => SetLayoutItem(item);
    }

    private sealed class ToolProbe : LayoutAnchorableTabItem
    {
        internal void ReleaseHook(PointerReleasedEventArgs args) => base.OnMouseLeftButtonUp(args);
    }

    private sealed class TitleProbe : AnchorablePaneTitle
    {
        internal void ReleaseHook(PointerReleasedEventArgs args) => base.OnMouseLeftButtonUp(args);
    }
}
