using System.Collections.ObjectModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;

namespace Dock.Serializer.UnitTests;

internal static class OverlayLayoutBuilder
{
    public static OverlayDock CreateLayout()
    {
        var background = new DocumentDock
        {
            Id = "Background",
            Title = "Background",
            VisibleDockables = new ObservableCollection<IDockable>
            {
                new Document { Id = "Doc1", Title = "Doc1" }
            }
        };

        var panel1 = new OverlayPanel
        {
            Id = "Panel1",
            Title = "Panel1",
            X = 10,
            Y = 20,
            Width = 300,
            Height = 200,
            VisibleDockables = new ObservableCollection<IDockable>
            {
                new DocumentDock
                {
                    Id = "Panel1Dock",
                    Title = "Panel1Dock",
                    VisibleDockables = new ObservableCollection<IDockable>
                    {
                        new Document { Id = "DocA", Title = "DocA" }
                    }
                }
            }
        };

        var panel2 = new OverlayPanel
        {
            Id = "Panel2",
            Title = "Panel2",
            X = 350,
            Y = 30,
            Width = 320,
            Height = 220,
            VisibleDockables = new ObservableCollection<IDockable>
            {
                new DocumentDock
                {
                    Id = "Panel2Dock",
                    Title = "Panel2Dock",
                    VisibleDockables = new ObservableCollection<IDockable>
                    {
                        new Document { Id = "DocB", Title = "DocB" }
                    }
                }
            }
        };

        var splitter = new OverlaySplitter
        {
            Id = "Splitter1",
            PanelBefore = panel1,
            PanelAfter = panel2,
            Thickness = 6.0
        };

        var group = new OverlaySplitterGroup
        {
            Id = "Group1",
            Title = "Group1",
            Panels = new ObservableCollection<IOverlayPanel> { panel1, panel2 },
            Splitters = new ObservableCollection<IOverlaySplitter> { splitter }
        };

        panel1.SplitterGroup = group;
        panel2.SplitterGroup = group;
        splitter.Owner = group;

        var overlayDock = new OverlayDock
        {
            Id = "Overlay",
            Title = "Overlay",
            VisibleDockables = new ObservableCollection<IDockable> { background, panel1, panel2 },
            SplitterGroups = new ObservableCollection<IOverlaySplitterGroup> { group }
        };

        background.Owner = overlayDock;
        panel1.Owner = overlayDock;
        panel2.Owner = overlayDock;
        group.Owner = overlayDock;

        return overlayDock;
    }
}