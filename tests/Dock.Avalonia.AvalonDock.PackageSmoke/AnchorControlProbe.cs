using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Model.Controls;

namespace PackageConsumer;

public sealed class AnchorManagerProbe : DockingManager
{
    public int[] SideChanges { get; } = new int[4];
    protected override void OnLeftSidePanelChanged(AvaloniaPropertyChangedEventArgs e) { SideChanges[0]++; base.OnLeftSidePanelChanged(e); }
    protected override void OnTopSidePanelChanged(AvaloniaPropertyChangedEventArgs e) { SideChanges[1]++; base.OnTopSidePanelChanged(e); }
    protected override void OnRightSidePanelChanged(AvaloniaPropertyChangedEventArgs e) { SideChanges[2]++; base.OnRightSidePanelChanged(e); }
    protected override void OnBottomSidePanelChanged(AvaloniaPropertyChangedEventArgs e) { SideChanges[3]++; base.OnBottomSidePanelChanged(e); }
}

internal static class AnchorControlProbe
{
    internal static void Verify()
    {
        AnchorControlWindow window = new();
        AnchorManagerProbe manager = window.FindControl<AnchorManagerProbe>("Manager")!;
        try
        {
            window.Show(); Update();
            LayoutAnchorSideControl[] sides = { manager.LeftSidePanel!, manager.TopSidePanel!, manager.RightSidePanel!, manager.BottomSidePanel! };
            if (manager.SideChanges.Any(count => count != 1)) throw new Exception("Packaged side creation did not invoke the external manager hooks.");
            foreach (LayoutAnchorSideControl side in sides)
            {
                LayoutAnchorSide model = (LayoutAnchorSide)side.Model;
                if (!ReferenceEquals(side.Tag, model) || side.IsLeftSide != (model.Side == AnchorSide.Left) ||
                    side.IsTopSide != (model.Side == AnchorSide.Top) || side.IsRightSide != (model.Side == AnchorSide.Right) ||
                    side.IsBottomSide != (model.Side == AnchorSide.Bottom))
                    throw new Exception("The packaged side flags or compiled public styles did not match the native edge.");
                foreach (LayoutAnchorGroupControl group in side.Children)
                foreach (LayoutAnchorControl anchor in group.Children)
                {
                    if (!ReferenceEquals(group.Model, group.Tag) || !ReferenceEquals(anchor.Model, anchor.Tag) ||
                        !ReferenceEquals(((LayoutAnchorable)anchor.Model).DockModel, anchor.DataContext) || anchor.Side != model.Side ||
                        !anchor.GetVisualAncestors().Contains(side))
                        throw new Exception("The packaged group and anchor controls did not present the existing model hierarchy.");
                }
            }
            LayoutAnchorGroupControl left = sides[0].Children[0];
            LayoutAnchorControl first = left.Children[0], second = left.Children[1];
            LayoutAnchorable tool = (LayoutAnchorable)first.Model;
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            document.IsActive = true;
            tool.IsSelected = true; Update();
            if (tool.IsSelected || !ReferenceEquals(manager.Layout.ActiveContent, document) ||
                !ReferenceEquals(((IRootDock)manager.DockLayout).PinnedDock?.ActiveDockable, tool.DockModel))
                throw new Exception("The packaged anchor did not open a passive native preview for selection.");
            Control view = manager.GetLayoutItemFromModel(tool)!.View;
            Point start = Center(first), target = Center(second);
            window.MouseMove(start); window.MouseDown(start, MouseButton.Left);
            window.MouseMove(target, RawInputModifiers.LeftMouseButton); window.MouseUp(target, MouseButton.Left); Update();
            if (!ReferenceEquals(left.Children[0], second) || !ReferenceEquals(left.Children[1], first) ||
                !ReferenceEquals(manager.DockLayout.LeftPinnedDockables![0], ((LayoutAnchorable)second.Model).DockModel) || !tool.IsActive)
                throw new Exception("The packaged anchor lost native pinned reordering or activation.");
            if (!manager.GetVisualDescendants().OfType<LayoutAnchorableControl>().Single(control => ReferenceEquals(control.Model, tool))
                .GetVisualDescendants().Contains(view))
                throw new Exception("The native preview replaced the packaged tool's existing content view.");

            manager.AnchorTemplate = (IControlTemplate)window.Resources["CustomAnchor"]!;
            manager.AnchorGroupTemplate = (IControlTemplate)window.Resources["CustomGroup"]!;
            manager.AnchorSideTemplate = (IControlTemplate)window.Resources["CustomSide"]!;
            tool.Title = "Changed"; Update();
            if (!first.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Name == "CustomAnchorHeader" && text.Text == "Changed") ||
                !left.GetVisualDescendants().OfType<ItemsControl>().Any(items => items.Name == "CustomGroupItems") ||
                !sides[0].GetVisualDescendants().OfType<ItemsControl>().Any(items => items.Name == "CustomSideItems"))
                throw new Exception("The packaged manager templates did not update existing controls through compiled bindings.");
            manager.ClearValue(DockingManager.AnchorSideTemplateProperty);
            manager.ClearValue(DockingManager.AnchorGroupTemplateProperty);
            manager.ClearValue(DockingManager.AnchorTemplateProperty); Update();
            int changes = manager.SideChanges[0];
            manager.LeftSidePanel = null; Update();
            if (manager.GetVisualDescendants().Contains(sides[0])) throw new Exception("The packaged side property did not remove its presentation.");
            manager.LeftSidePanel = sides[0]; Update();
            if (manager.SideChanges[0] != changes + 2 || !manager.GetVisualDescendants().Contains(first))
                throw new Exception("Restoring the packaged side replaced the public anchor or missed the external override.");
            object? content = tool.Content;
            using StringWriter xml = new();
            XmlLayoutSerializer serializer = new(manager);
            serializer.Serialize(xml); serializer.Deserialize(new StringReader(xml.ToString())); Update();
            LayoutAnchorControl restored = manager.LeftSidePanel!.Children.SelectMany(group => group.Children)
                .Single(anchor => ((LayoutAnchorable)anchor.Model).ContentId == "left-a");
            if (ReferenceEquals(manager.LeftSidePanel, sides[0]) || !ReferenceEquals(((LayoutAnchorable)restored.Model).Content, content))
                throw new Exception("Packaged XML replacement did not update public controls and preserve application content.");
            window.Content = null; Update(); window.Content = manager; Update();
            if (!manager.GetVisualDescendants().Contains(restored)) throw new Exception("The packaged anchor did not survive visual reattachment.");
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }

        Point Center(Control control) => control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), window)!.Value;
        void Update() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
    }
}
