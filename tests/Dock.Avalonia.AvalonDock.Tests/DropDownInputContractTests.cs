using System.Collections.Generic;
using System.Reactive.Concurrency;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using ReactiveUI.Reactive;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class MenuControlTests
{
    [AvaloniaTheory]
    [InlineData(false, true, "plain")]
    [InlineData(false, false, "plain")]
    [InlineData(false, true, "menu")]
    [InlineData(false, true, "flyout")]
    [InlineData(false, true, "foreign")]
    [InlineData(true, true, "plain")]
    [InlineData(true, false, "menu")]
    public void Area_Preview_Hook_Preserves_Handled_And_Uses_The_Current_Menu(
        bool handled, bool enabled, string sourceKind)
    {
        Border child = new() { Background = Brushes.Transparent };
        if (sourceKind == "menu") child.ContextMenu = CreateMenu();
        if (sourceKind == "flyout") child.ContextFlyout = new Flyout();
        ContextMenu menu = CreateMenu();
        object data = new();
        AreaProbe area = new()
        {
            Content = child, IsEnabled = enabled,
            DropDownContextMenu = menu, DropDownContextMenuDataContext = data
        };
        using MenuWindow window = new(area);
        Control source = sourceKind == "foreign" ? new Border() : child;
        Pointer pointer = new(1, PointerType.Mouse, true);
        PointerReleasedEventArgs args = new(source, pointer, source, default, 0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.RightButtonReleased),
            KeyModifiers.None, MouseButton.Right) { Handled = handled };

        area.ReleaseHook(args);

        Assert.Equal(handled, args.Handled);
        Assert.Equal(!handled, menu.IsOpen);
        if (!handled) Assert.Same(data, menu.DataContext);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Area_Preview_Opening_Allows_Application_Release_Handlers_Without_Reopening(bool nested)
    {
        Border child = new() { Background = Brushes.Transparent };
        ContextMenu menu = CreateMenu();
        AreaProbe area = new() { Width = 200, Height = 80, Content = child, DropDownContextMenu = menu };
        AreaProbe outer = new() { Content = area, DropDownContextMenu = nested ? CreateMenu() : null };
        List<string> events = new();
        menu.Opening += (_, _) => events.Add("inner");
        if (outer.DropDownContextMenu is { } outerMenu) outerMenu.Opening += (_, _) => events.Add("outer");
        child.AddHandler(InputElement.PointerReleasedEvent, (_, args) =>
        {
            Assert.False(args.Handled);
            events.Add("child release");
        }, RoutingStrategies.Bubble);
        outer.AddHandler(InputElement.PointerReleasedEvent, (_, args) =>
        {
            Assert.False(args.Handled);
            events.Add("outer release");
        }, RoutingStrategies.Bubble);
        using MenuWindow window = new(outer);

        Click(child, MouseButton.Right);

        Assert.Equal(nested
            ? new[] { "outer", "inner", "child release", "outer release" }
            : new[] { "inner", "child release", "outer release" }, events);
        Assert.True(menu.IsOpen);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Button_Explicit_Clicks_Preserve_Toggle_Order_And_Do_Not_Reopen_An_Open_Menu(bool enabled)
    {
        ContextMenu menu = CreateMenu();
        ButtonProbe button = new() { IsEnabled = enabled, DropDownContextMenu = menu };
        List<bool?> checkedAtClick = new();
        button.Click += (_, _) => checkedAtClick.Add(button.IsChecked);
        int openings = 0;
        menu.Opening += (_, _) => openings++;
        using MenuWindow window = new(button);

        button.InvokeClick();
        Assert.True(menu.IsOpen);
        Assert.True(button.IsChecked);
        object updatedData = new();
        button.DropDownContextMenuDataContext = updatedData;
        button.InvokeClick();
        Assert.True(menu.IsOpen);
        Assert.False(button.IsChecked);
        Assert.Same(updatedData, menu.DataContext);
        button.IsThreeState = true;
        button.InvokeClick();
        button.InvokeClick();
        Assert.Null(button.IsChecked);
        Assert.Equal(new bool?[] { true, false, true, null }, checkedAtClick);
        Assert.Equal(1, openings);
        menu.Close();
        Assert.False(button.IsChecked);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Button_Click_Handlers_Can_Replace_The_Command_And_Handled_Does_Not_Suppress_It(bool enabled)
    {
        object parameter = new();
        int firstCalls = 0;
        List<object?> arguments = new();
        using var first = ReactiveCommand.Create(() => firstCalls++, outputScheduler: ImmediateScheduler.Instance);
        using var replacement = ReactiveCommand.Create<object?>(arguments.Add, outputScheduler: ImmediateScheduler.Instance);
        ButtonProbe button = new() { IsEnabled = enabled, Command = first };
        using MenuWindow window = new(button);
        button.Click += (_, args) =>
        {
            Assert.True(button.IsChecked);
            button.Command = replacement;
            button.CommandParameter = parameter;
            args.Handled = true;
        };

        button.InvokeClick();

        Assert.Equal(0, firstCalls);
        Assert.Same(parameter, Assert.Single(arguments));
    }

    [AvaloniaTheory]
    [InlineData("Button")]
    [InlineData("Area")]
    public void An_Open_Dropdown_Retains_Resource_Updates_Without_Owning_The_Normal_ContextMenu(string key)
    {
        using Workspace workspace = new(key);
        MenuItem item = (MenuItem)workspace.Menu.Items[0]!;
        item.Bind(MenuItem.HeaderProperty, new DynamicResourceExtension("LiveHeader"));
        workspace.Control.Resources["LiveHeader"] = "Before";

        workspace.Open();

        Assert.Null(workspace.Control.ContextMenu);
        Assert.Equal("Before", item.Header);
        workspace.Control.Resources["LiveHeader"] = "After";
        Dispatcher.UIThread.RunJobs();
        Assert.Equal("After", item.Header);
        Assert.True(workspace.Menu.IsOpen);
    }
}
