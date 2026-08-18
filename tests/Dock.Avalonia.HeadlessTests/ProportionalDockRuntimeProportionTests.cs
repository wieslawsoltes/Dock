using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;
using AvaloniaFactory = Dock.Model.Avalonia.Factory;
using MvvmFactory = Dock.Model.Mvvm.Factory;

namespace Dock.Avalonia.HeadlessTests;

public class ProportionalDockRuntimeProportionTests
{
    [AvaloniaFact]
    public void AvaloniaModelProportionChanges_UpdateRealizedPanelChildren()
    {
        VerifyRuntimeProportionUpdates(new AvaloniaFactory());
    }

    [AvaloniaFact]
    public void MvvmModelProportionChanges_UpdateRealizedPanelChildren()
    {
        VerifyRuntimeProportionUpdates(new MvvmFactory());
    }

    private static void VerifyRuntimeProportionUpdates(IFactory factory)
    {
        var firstDock = CreateDocumentDock(factory, "first", 0.25);
        var secondDock = CreateDocumentDock(factory, "second", 0.75);
        var layout = factory.CreateProportionalDock();
        layout.VisibleDockables = factory.CreateList<IDockable>(
            firstDock,
            factory.CreateProportionalDockSplitter(),
            secondDock);
        layout.ActiveDockable = firstDock;
        layout.Orientation = Orientation.Horizontal;

        var root = factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>(layout);
        root.ActiveDockable = layout;
        root.DefaultDockable = layout;

        factory.InitLayout(root);

        var dockControl = new DockControl
        {
            Factory = factory,
            Layout = root,
            InitializeFactory = false,
            InitializeLayout = false
        };
        var window = new Window
        {
            Width = 800,
            Height = 500,
            Content = dockControl
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var panel = dockControl.GetVisualDescendants()
                .OfType<ProportionalStackPanel>()
                .Single();
            var firstPresenter = GetPresenter(panel, firstDock);
            var secondPresenter = GetPresenter(panel, secondDock);

            Assert.Equal(0.25, ProportionalStackPanel.GetProportion(firstPresenter));
            Assert.Equal(0.75, ProportionalStackPanel.GetProportion(secondPresenter));

            firstDock.Proportion = 0.75;
            secondDock.Proportion = 0.25;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Assert.Equal(0.75, ProportionalStackPanel.GetProportion(firstPresenter));
            Assert.Equal(0.25, ProportionalStackPanel.GetProportion(secondPresenter));
            Assert.True(firstPresenter.Bounds.Width > secondPresenter.Bounds.Width);

            ProportionalStackPanel.SetProportion(firstPresenter, 0.6);
            ProportionalStackPanel.SetProportion(secondPresenter, 0.4);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(0.6, firstDock.Proportion);
            Assert.Equal(0.4, secondDock.Proportion);
        }
        finally
        {
            window.Close();
        }
    }

    private static IDocumentDock CreateDocumentDock(IFactory factory, string id, double proportion)
    {
        var document = factory.CreateDocument();
        document.Id = $"{id}-document";

        var dock = factory.CreateDocumentDock();
        dock.Id = id;
        dock.Proportion = proportion;
        dock.VisibleDockables = factory.CreateList<IDockable>(document);
        dock.ActiveDockable = document;
        return dock;
    }

    private static ContentPresenter GetPresenter(ProportionalStackPanel panel, IDockable dockable)
    {
        return panel.Children
            .OfType<ContentPresenter>()
            .Single(presenter => ReferenceEquals(presenter.DataContext, dockable));
    }
}
