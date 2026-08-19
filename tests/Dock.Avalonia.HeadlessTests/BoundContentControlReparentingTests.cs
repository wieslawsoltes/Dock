using System;
using Avalonia.Controls;
using Avalonia.Controls.Recycling;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class BoundContentControlReparentingTests
{
    [AvaloniaFact]
    public void DockControl_PropagatesConfiguredControlRecyclingAcrossFactory()
    {
        var factory = new Factory();
        var recycling = new ControlRecycling { TryToUseIdAsKey = true };
        var firstRoot = new RootDock
        {
            Id = "FirstRoot",
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        var secondRoot = new RootDock
        {
            Id = "SecondRoot",
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        var firstDockControl = new DockControl { Layout = firstRoot };
        ControlRecyclingDataTemplate.SetControlRecycling(firstDockControl, recycling);
        var secondDockControl = new DockControl { Layout = secondRoot };
        var firstWindow = new Window { Content = firstDockControl };
        var secondWindow = new Window { Content = secondDockControl };

        firstWindow.Show();
        try
        {
            firstDockControl.ApplyTemplate();
            var sharedRecycling = Assert.IsType<ControlRecycling>(
                ControlRecyclingDataTemplate.GetControlRecycling(firstDockControl));

            secondWindow.Show();
            secondDockControl.ApplyTemplate();

            Assert.True(sharedRecycling.TryToUseIdAsKey);
            Assert.Same(sharedRecycling, ControlRecyclingDataTemplate.GetControlRecycling(secondDockControl));
        }
        finally
        {
            secondWindow.Close();
            firstWindow.Close();
        }
    }

    [AvaloniaFact]
    public void MovingDocumentWithBoundControlContentBetweenDocks_ReparentsSharedControl()
    {
        var factory = new Factory();
        var model = new BoundControlContext();
        var buildCount = 0;
        var document = new Document
        {
            Id = "Document",
            Title = "Document",
            Context = model,
            Content = new Func<IServiceProvider, object>(_ =>
            {
                buildCount++;
                return new BoundContentControlView();
            })
        };
        var source = new DocumentDock
        {
            Id = "Source",
            VisibleDockables = factory.CreateList<IDockable>(document),
            ActiveDockable = document
        };
        var target = new DocumentDock
        {
            Id = "Target",
            VisibleDockables = factory.CreateList<IDockable>()
        };
        var documents = new ProportionalDock
        {
            Id = "Documents",
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>(source, target),
            ActiveDockable = source
        };
        var root = new RootDock
        {
            Id = "Root",
            VisibleDockables = factory.CreateList<IDockable>(documents),
            ActiveDockable = documents,
            DefaultDockable = documents
        };
        var dockControl = new DockControl
        {
            Factory = factory,
            Layout = root,
            InitializeFactory = true,
            InitializeLayout = true
        };
        var window = new Window
        {
            Width = 900,
            Height = 600,
            Content = dockControl
        };

        window.Show();
        try
        {
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Assert.IsType<ControlRecycling>(ControlRecyclingDataTemplate.GetControlRecycling(dockControl));
            Assert.NotNull(model.SharedControl.GetVisualParent());
            Assert.Equal(1, buildCount);

            factory.MoveDockable(source, target, document, null);
            window.UpdateLayout();
            Assert.Equal(1, buildCount);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Assert.Same(target, document.Owner);
            Assert.NotNull(model.SharedControl.GetVisualParent());
            Assert.Equal(1, buildCount);
        }
        finally
        {
            window.Close();
        }
    }

}

public sealed class BoundControlContext
{
    public Button SharedControl { get; } = new() { Content = "Shared control" };
}
