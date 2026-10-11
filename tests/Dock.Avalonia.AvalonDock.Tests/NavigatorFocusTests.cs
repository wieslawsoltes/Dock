using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Layout;
using Dock.Controls.DeferredContentControl;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class NavigatorTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Pending_Content_Focus_Does_Not_Replace_A_New_Focus_Choice_Even_When_The_Event_Is_Handled(bool floating)
    {
        TextBox search = new() { Text = "Application search" };
        using Workspace workspace = new(floating ? null : search);
        TopLevel owner = workspace.View;
        AvaloniaObject schedulingScope = workspace.Manager;
        if (floating)
        {
            workspace.Documents[1].Float();
            owner = (TopLevel)workspace.Documents[1].FindParent<LayoutFloatingWindow>()!.DockWindow.Host!;
            DeferredContentScheduling.SetTimeline(owner, new DeferredContentPresentationTimeline { InitialDelay = TimeSpan.FromHours(1) });
            owner.GetVisualDescendants().OfType<Panel>().First().Children.Add(search);
            schedulingScope = owner;
        }
        TextBox delayed = new() { Text = "Delayed document" };
        HoldNavigatorContent(workspace, delayed, schedulingScope);
        search.GotFocus += (_, args) => args.Handled = true;
        Assert.True(search.Focus());
        Assert.True(workspace.Documents[1].IsActive);
        DeferredContentScheduling.SetTimeline(schedulingScope, new DeferredContentPresentationTimeline
        {
            FollowUpDelay = TimeSpan.Zero,
            MaxPresentationsPerPass = 1,
            RevealDuration = TimeSpan.Zero
        });
        Dispatcher.UIThread.RunJobs();
        Assert.Same(owner, TopLevel.GetTopLevel(delayed));
        Assert.Same(search, owner.FocusManager!.GetFocusedElement());
    }

    [AvaloniaTheory]
    [InlineData("deactivate")]
    [InlineData("replace")]
    [InlineData("detach")]
    public void Invalidated_Focus_Requests_Do_Not_Focus_Content_When_It_Is_Later_Attached_Elsewhere(string change)
    {
        using Workspace workspace = new();
        TextBox delayed = new() { Text = "Previous document" };
        HoldNavigatorContent(workspace, delayed);
        switch (change)
        {
            case "deactivate": workspace.Documents[0].IsActive = true; break;
            case "replace": workspace.Documents[1].Content = new TextBox(); break;
            case "detach": workspace.View.Content = null; break;
        }
        TextBox current = new() { Text = "Current editor" };
        StackPanel panel = new() { Children = { current } };
        Window other = new() { Content = panel, Width = 400, Height = 300 };
        try
        {
            other.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.True(current.Focus());
            panel.Children.Add(delayed);
            Dispatcher.UIThread.RunJobs();
            Assert.Same(other, TopLevel.GetTopLevel(delayed));
            Assert.Same(current, other.FocusManager!.GetFocusedElement());
        }
        finally { other.Close(); }
    }

    [AvaloniaFact]
    public void Retained_Unrealized_Content_Does_Not_Retain_A_Closed_Manager_With_Pending_Focus()
    {
        TextBox retained = new() { Text = "Retained document" };
        WeakReference<DockingManager> reference = ReleaseManagerWithPendingFocus(retained);
        for (int index = 0; index < 5; index++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        Assert.False(reference.TryGetTarget(out _));
        GC.KeepAlive(retained);
    }

    private static void HoldNavigatorContent(Workspace workspace, Control content, AvaloniaObject? schedulingScope = null)
    {
        workspace.Documents[0].IsActive = true;
        for (int index = 0; index < workspace.Documents.Length; index++)
            workspace.Documents[index].LastActivationTimeStamp = new DateTime(2020, 1, 3 - index);
        DeferredContentScheduling.SetTimeline(schedulingScope ?? workspace.Manager, new DeferredContentPresentationTimeline
        {
            InitialDelay = TimeSpan.FromHours(1),
            RevealDuration = TimeSpan.Zero
        });
        workspace.Documents[1].Content = content;
        workspace.Manager.Open();
        Assert.Equal(workspace.Documents[1].Title, workspace.Manager.Last!.SelectedDocument?.Title);
        Assert.True(workspace.Manager.Last!.SelectedDocument!.ActivateCommand!.CanExecute(null));
        workspace.Manager.Last!.MouseMove(default);
        workspace.Manager.Last!.KeyReleaseQwerty(PhysicalKey.ControlLeft, RawInputModifiers.None);
        Assert.False(workspace.Manager.Last!.IsVisible);
        Assert.True(workspace.Documents[1].IsActive);
        Dispatcher.UIThread.RunJobs();
        Assert.True(workspace.Documents[1].IsActive);
        if (workspace.Documents[1].FindParent<LayoutFloatingWindow>() is null)
            Assert.Same(workspace.Documents[1], workspace.Manager.Layout.ActiveContent);
        Assert.Null(TopLevel.GetTopLevel(content));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<DockingManager> ReleaseManagerWithPendingFocus(Control content)
    {
        using Workspace workspace = new();
        HoldNavigatorContent(workspace, content);
        return new(workspace.Manager);
    }
}
