using System;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model.Core;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestCaseHelpers;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class DockLayoutEventLeakTests
{
    [ReleaseFact]
    public void GridDockControl_Loaded_AppliesDefinitions_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            context.GridDock.RowDefinitions = "Auto,*";
            context.GridDock.ColumnDefinitions = "*,Auto";
            context.GridDock.VisibleDockables?.Add(context.Document);

            var control = new GridDockControl
            {
                DataContext = context.GridDock
            };

            var window = new Window { Content = control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            var itemsControl = FindTemplateChild<ItemsControl>(control, "PART_ItemsControl");
            var grid = itemsControl?.ItemsPanelRoot as Grid;

            var controlRef = new WeakReference(control);
            var rowCount = grid?.RowDefinitions?.Count ?? 0;
            var columnCount = grid?.ColumnDefinitions?.Count ?? 0;

            var result = new GridDockEventLeakResult(controlRef, window, rowCount, columnCount);
            window.Content = null;
            DrainDispatcher();
            ClearFactoryCaches(context.Factory);
            ClearInputState(window);
            ResetDispatcherForUnitTests();
            control = null;
            return result;
        });

        Assert.True(result.RowCount >= 2, "GridDockControl did not apply row definitions.");
        Assert.True(result.ColumnCount >= 2, "GridDockControl did not apply column definitions.");
        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.WindowKeepAlive);
    }

    [ReleaseFact]
    public void ProportionalDockControl_ContainerPrepared_BindsProportion_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            var dockable = context.Document;
            dockable.Proportion = 0.42;
            context.ProportionalDock.VisibleDockables?.Add(dockable);

            var control = new ProportionalDockControl
            {
                DataContext = context.ProportionalDock
            };

            var window = new Window { Content = control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            var itemsControl = FindTemplateChild<ItemsControl>(control, "PART_ItemsControl");
            var container = itemsControl?.ContainerFromIndex(0) as Control;
            var proportion = container is null ? double.NaN : ProportionalStackPanel.GetProportion(container);

            var controlRef = new WeakReference(control);
            var expectedProportion = dockable.Proportion;

            var result = new ProportionalDockEventLeakResult(controlRef, window, proportion, expectedProportion);
            window.Content = null;
            DrainDispatcher();
            ClearFactoryCaches(context.Factory);
            ClearInputState(window);
            ResetDispatcherForUnitTests();
            control = null;
            return result;
        });

        Assert.True(!double.IsNaN(result.Proportion), "ProportionalDockControl did not set proportion.");
        Assert.Equal(result.ExpectedProportion, result.Proportion, 3);
        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.WindowKeepAlive);
    }

    [ReleaseFact]
    public void SplitViewDockControl_PaneEvents_UpdateModel_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            context.SplitViewDock.PaneDockable = context.ToolDock;
            context.SplitViewDock.ContentDockable = context.DocumentDock;
            context.SplitViewDock.DisplayMode = Dock.Model.Core.SplitViewDisplayMode.Overlay;
            context.SplitViewDock.UseLightDismissOverlayMode = true;
            context.SplitViewDock.IsPaneOpen = true;

            var control = new SplitViewDockControl
            {
                DataContext = context.SplitViewDock
            };

            var window = new Window { Content = control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            var splitView = FindTemplateChild<SplitView>(control, "PART_SplitView");
            if (splitView is not null)
            {
                splitView.IsPaneOpen = false;
                splitView.IsPaneOpen = true;
            }
            DrainDispatcher();

            var controlRef = new WeakReference(control);
            var isPaneOpen = context.SplitViewDock.IsPaneOpen;

            var result = new SplitViewDockEventLeakResult(controlRef, window, isPaneOpen);
            window.Content = null;
            DrainDispatcher();
            ClearFactoryCaches(context.Factory);
            ClearInputState(window);
            ResetDispatcherForUnitTests();
            control = null;
            return result;
        });

        Assert.True(result.IsPaneOpen, "SplitViewDockControl did not update model IsPaneOpen.");
        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.WindowKeepAlive);
    }

    private sealed record GridDockEventLeakResult(
        WeakReference ControlRef,
        Window WindowKeepAlive,
        int RowCount,
        int ColumnCount);

    private sealed record ProportionalDockEventLeakResult(
        WeakReference ControlRef,
        Window WindowKeepAlive,
        double Proportion,
        double ExpectedProportion);

    private sealed record SplitViewDockEventLeakResult(
        WeakReference ControlRef,
        Window WindowKeepAlive,
        bool IsPaneOpen);
}
