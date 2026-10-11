using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using ReactiveUI.Reactive;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingKeyboardOptions : ReactiveObject
{
    private bool _enabled;
    public bool Enabled { get => _enabled; set => this.RaiseAndSetIfChanged(ref _enabled, value); }
}

public class FloatingKeyboardTests
{
    [AvaloniaFact]
    public void A_Literal_Xaml_Option_Can_Be_Constructed_From_A_Consumer_Assembly()
    {
        FloatingFixture view = new();
        try
        {
            DockingManager manager = view.FindControl<DockingManager>("Manager")!;
            Assert.True(manager.AllowMovingFloatingWindowWithKeyboard);
            manager.AllowMovingFloatingWindowWithKeyboard = false;
            Assert.False(manager.AllowMovingFloatingWindowWithKeyboard);
        }
        finally { view.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compiled_Option_Updates_Live_And_Arrow_Keys_Move_The_Current_Floating_Host(bool tool)
    {
        using Workspace workspace = new(tool);
        LayoutFloatingWindowControl host = workspace.Float();
        Assert.False(new DockingManager().AllowMovingFloatingWindowWithKeyboard);
        Assert.False(workspace.Manager.AllowMovingFloatingWindowWithKeyboard);
        host.Position = new PixelPoint(100, 120);
        Assert.True(((Control)workspace.Content.Content!).Focus());
        Press(host, PhysicalKey.ArrowRight);
        Assert.Equal(new PixelPoint(100, 120), host.Position);

        workspace.Options.Enabled = true;
        Assert.True(workspace.Manager.AllowMovingFloatingWindowWithKeyboard);
        Press(host, PhysicalKey.ArrowRight);
        Assert.Equal(new PixelPoint(125, 120), host.Position);
        Press(host, PhysicalKey.ArrowDown);
        Assert.Equal(new PixelPoint(125, 145), host.Position);
        Press(host, PhysicalKey.ArrowLeft);
        Assert.Equal(new PixelPoint(100, 145), host.Position);
        Press(host, PhysicalKey.ArrowUp);
        Assert.Equal(new PixelPoint(100, 120), host.Position);
        Press(host, PhysicalKey.Space);
        Assert.Equal(new PixelPoint(100, 120), host.Position);

        workspace.Options.Enabled = false;
        Press(host, PhysicalKey.ArrowDown);
        Assert.Equal(new PixelPoint(100, 120), host.Position);
    }

    [AvaloniaFact]
    public void Movement_Uses_Desktop_Scaling_And_Persists_Through_Native_Save_And_Xml()
    {
        using Workspace workspace = new();
        LayoutFloatingWindowControl host = workspace.Float();
        workspace.Options.Enabled = true;
        host.SetRenderScaling(2);
        Assert.Equal(2, host.DesktopScaling);
        host.Position = new PixelPoint(100, 120);
        Assert.True(((Control)workspace.Content.Content!).Focus());
        Press(host, PhysicalKey.ArrowRight);
        Press(host, PhysicalKey.ArrowDown);
        Assert.Equal(new PixelPoint(150, 170), host.Position);
        LayoutFloatingWindow model = (LayoutFloatingWindow)host.Model;
        model.DockWindow.Save();
        Assert.Equal(150, workspace.Content.FloatingLeft);
        Assert.Equal(170, workspace.Content.FloatingTop);
        XmlLayoutSerializer serializer = new(workspace.Manager);
        using StringWriter writer = new();
        serializer.Serialize(writer);
        serializer.Deserialize(new StringReader(writer.ToString()));
        Dispatcher.UIThread.RunJobs();
        LayoutFloatingWindowControl replacement = Assert.Single(workspace.Manager.FloatingWindows);
        Assert.NotSame(host, replacement);
        Assert.Equal(new PixelPoint(150, 170), replacement.Position);
    }

    [AvaloniaFact]
    public void Handled_Editor_Keys_Do_Not_Move_The_Window_And_Movement_Does_Not_Handle_The_Event()
    {
        using Workspace workspace = new();
        TextBox editor = new() { Text = "abc", CaretIndex = 1 };
        workspace.Content.Content = editor;
        LayoutFloatingWindowControl host = workspace.Float();
        workspace.Options.Enabled = true;
        host.Position = new PixelPoint(100, 120);
        Assert.True(editor.Focus());
        Press(host, PhysicalKey.ArrowRight);
        Assert.Equal(2, editor.CaretIndex);
        Assert.Equal(new PixelPoint(100, 120), host.Position);

        Border target = new() { Focusable = true };
        workspace.Content.Content = target;
        Dispatcher.UIThread.RunJobs();
        host.UpdateLayout();
        Assert.True(target.Focus());
        bool? handled = null;
        host.AddHandler(InputElement.KeyDownEvent, (_, args) => handled = args.Handled,
            RoutingStrategies.Bubble, handledEventsToo: true);
        Press(host, PhysicalKey.ArrowRight, RawInputModifiers.Control | RawInputModifiers.Shift);
        Assert.Equal(new PixelPoint(125, 120), host.Position);
        Assert.False(handled);
    }

    [AvaloniaFact]
    public void The_Option_Is_Per_Manager_And_Still_Applies_After_Hiding_And_Reattaching()
    {
        using Workspace first = new(tool: true);
        using Workspace second = new();
        LayoutFloatingWindowControl firstHost = first.Float();
        LayoutFloatingWindowControl secondHost = second.Float();
        first.Options.Enabled = true;
        firstHost.Position = secondHost.Position = new PixelPoint(100, 120);
        Assert.True(((Control)second.Content.Content!).Focus());
        Press(secondHost, PhysicalKey.ArrowLeft);
        Assert.Equal(new PixelPoint(100, 120), secondHost.Position);
        LayoutAnchorable tool = (LayoutAnchorable)first.Content;
        tool.Hide();
        tool.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.True(((Control)tool.Content!).Focus());
        Press(firstHost, PhysicalKey.ArrowLeft);
        Assert.Equal(new PixelPoint(75, 120), firstHost.Position);

        first.View.Content = null;
        first.View.Content = first.Manager;
        Dispatcher.UIThread.RunJobs();
        LayoutFloatingWindowControl replacement = Assert.Single(first.Manager.FloatingWindows);
        Assert.True(first.Manager.AllowMovingFloatingWindowWithKeyboard);
        Assert.True(((Control)tool.Content!).Focus());
        Press(replacement, PhysicalKey.ArrowUp);
        Assert.Equal(new PixelPoint(75, 95), replacement.Position);
    }

    private static void Press(Window host, PhysicalKey key, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        host.KeyPressQwerty(key, modifiers);
        host.KeyReleaseQwerty(key, modifiers);
    }

    private sealed class Workspace : IDisposable
    {
        internal FloatingKeyboardOptions Options { get; } = new();
        internal FloatingKeyboardFixture View { get; } = new();
        internal DockingManager Manager { get; }
        internal LayoutContent Content { get; }

        internal Workspace(bool tool = false)
        {
            View.DataContext = Options;
            Manager = View.FindControl<DockingManager>("Manager")!;
            Content = Manager.Layout.Descendents().OfType<LayoutContent>().Single(content => content.ContentId == (tool ? "tool" : "document"));
            View.Show();
            Dispatcher.UIThread.RunJobs();
        }

        internal LayoutFloatingWindowControl Float()
        {
            Content.Float();
            Dispatcher.UIThread.RunJobs();
            return Assert.Single(Manager.FloatingWindows);
        }

        public void Dispose()
        {
            Manager.Layout.FloatingWindows.Clear();
            View.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
