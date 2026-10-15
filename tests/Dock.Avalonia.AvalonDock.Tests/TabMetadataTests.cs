using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class TabControlTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Native_Tab_Metadata_Updates_While_Docked_Or_Floating(bool tool, bool floating)
    {
        using Workspace workspace = new(tool);
        if (floating)
        {
            if (tool) workspace.Manager.DockFactory.FloatAllDockables(workspace.First.DockModel);
            else workspace.First.Float();
            workspace.Update();
        }
        Control scope = floating ? workspace.Manager.FloatingWindows.Single() : workspace.Manager;
        Control header = NativeTab(scope, workspace.First, tool);
        Control tab = NativeContainer(header, tool);
        workspace.First.ToolTip = "Document location";
        workspace.First.IsEnabled = false;
        workspace.Update();
        Assert.False(tab.IsEnabled);
        Assert.False(header.IsEffectivelyEnabled);
        Assert.Equal("Document location", ToolTip.GetTip(tab));
        Assert.Same(workspace.First.DockModel, tab.DataContext);

        TextBlock tip = new() { Text = "Rich tooltip content" };
        ToolTip.SetTip(workspace.Manager.GetLayoutItemFromModel(workspace.First)!, tip);
        workspace.First.IsEnabled = true;
        workspace.Update();
        Assert.True(tab.IsEnabled);
        Assert.True(header.IsEffectivelyEnabled);
        Assert.Same(tip, workspace.First.ToolTip);
        Assert.Same(tip, ToolTip.GetTip(tab));
        ToolTip.SetIsOpen(tab, true);
        workspace.Update();
        Assert.True(ToolTip.GetIsOpen(tab));
        Assert.True(tip.IsEffectivelyVisible);
        Assert.NotNull(tip.GetVisualParent());
        ToolTip.SetIsOpen(tab, false);
        workspace.First.ToolTip = null;
        workspace.Update();
        Assert.Null(ToolTip.GetTip(tab));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Disabled_Native_And_Custom_Tabs_Do_Not_Select_Or_Activate_Their_Content(bool tool)
    {
        using Workspace workspace = new(tool);
        workspace.Second.IsActive = true;
        workspace.First.IsEnabled = false;
        workspace.Update();
        Control header = NativeTab(workspace.Manager, workspace.First, tool);
        Click(NativeContainer(header, tool));
        Click(workspace.Tab);
        Assert.Same(workspace.Second, workspace.Manager.Layout.ActiveContent);
        Assert.False(workspace.First.IsSelected);
        Assert.True(workspace.Second.IsSelected);
        workspace.First.IsEnabled = true;
        workspace.Update();
        Click(header);
        Assert.Same(workspace.First, workspace.Manager.Layout.ActiveContent);
        Assert.True(workspace.First.IsSelected);
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Default_Tool_Icons_Update_In_Docked_Floating_And_AutoHide_Headers(int placement)
    {
        using Workspace workspace = new(true);
        LayoutAnchorable tool = Assert.IsType<LayoutAnchorable>(workspace.First);
        if (placement == 1) workspace.Manager.DockFactory.FloatAllDockables(tool.DockModel);
        else if (placement == 2) tool.ToggleAutoHide();
        workspace.Update();
        Control scope = placement == 1 ? workspace.Manager.FloatingWindows.Single() : workspace.Manager;
        Image image = scope.GetVisualDescendants().OfType<Image>().Single(candidate =>
            candidate.Classes.Contains("avalondock-tool-icon") && ReferenceEquals(candidate.DataContext, tool));
        Assert.Null(image.Source);
        Assert.False(image.IsVisible);
        IImage icon = CreateIcon(Brushes.Red);
        tool.IconSource = icon;
        workspace.Update();
        Assert.Same(icon, image.Source);
        Assert.True(image.IsEffectivelyVisible);
        Assert.Equal(16, image.Bounds.Width);
        IImage replacement = CreateIcon(Brushes.Blue);
        workspace.Manager.GetLayoutItemFromModel(tool)!.IconSource = replacement;
        workspace.Update();
        Assert.Same(replacement, image.Source);
        tool.IconSource = null;
        workspace.Update();
        Assert.Null(image.Source);
        Assert.False(image.IsVisible);
        Assert.Same(image, scope.GetVisualDescendants().OfType<Image>().Single(candidate =>
            candidate.Classes.Contains("avalondock-tool-icon") && ReferenceEquals(candidate.DataContext, tool)));
    }

    [AvaloniaTheory]
    [InlineData("DocumentTab", false)]
    [InlineData("LegacyDocumentTab", false)]
    [InlineData("ToolTab", true)]
    [InlineData("LegacyToolTab", true)]
    public void Custom_Tabs_In_Both_Namespaces_Follow_Their_Current_Models_Metadata(string key, bool tool)
    {
        using Workspace workspace = new(tool);
        workspace.First.IsEnabled = false;
        workspace.First.ToolTip = "First tooltip";
        Control tab = ((IDataTemplate)workspace.Window.Resources[key]!).Build(workspace.First)!;
        tab.DataContext = workspace.First;
        workspace.Tabs.Children.Clear();
        workspace.Tabs.Children.Add(tab);
        workspace.Update();
        Assert.False(tab.IsEnabled);
        Assert.Equal("First tooltip", ToolTip.GetTip(tab));
        workspace.Second.ToolTip = "Second tooltip";
        tab.DataContext = workspace.Second;
        workspace.Update();
        Assert.True(tab.IsEnabled);
        Assert.Equal("Second tooltip", ToolTip.GetTip(tab));
        workspace.First.ToolTip = "Old model";
        Assert.Equal("Second tooltip", ToolTip.GetTip(tab));
        workspace.Second.ToolTip = null;
        Assert.Null(ToolTip.GetTip(tab));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Header_Templates_Replace_The_Default_Icon_Without_Replacing_Tab_Metadata(bool tool)
    {
        using Workspace workspace = new(tool);
        workspace.First.IconSource = CreateIcon(Brushes.Red);
        workspace.First.ToolTip = "Keep this hint";
        workspace.Update();
        Control header = NativeTab(workspace.Manager, workspace.First, tool);
        Control tab = NativeContainer(header, tool);
        Assert.Equal(tool ? 1 : 0, header.GetVisualDescendants().OfType<Image>().Count());
        if (tool) workspace.Manager.AnchorableHeaderTemplate = (IDataTemplate)workspace.Window.Resources["ToolHeader"]!;
        else workspace.Manager.DocumentHeaderTemplate = (IDataTemplate)workspace.Window.Resources["DocumentHeader"]!;
        workspace.Update();
        Assert.Empty(header.GetVisualDescendants().OfType<Image>());
        Assert.Equal("Keep this hint", ToolTip.GetTip(tab));
        if (tool) workspace.Manager.AnchorableHeaderTemplate = null;
        else workspace.Manager.DocumentHeaderTemplate = null;
        workspace.Update();
        Assert.Equal(tool ? 1 : 0, header.GetVisualDescendants().OfType<Image>().Count());
        Assert.Equal("Keep this hint", ToolTip.GetTip(tab));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Retained_Native_Tab_Containers_Clear_Compatibility_Metadata_After_Layout_Replacement(bool tool)
    {
        using Workspace workspace = new(tool);
        workspace.First.ToolTip = "Old tooltip";
        workspace.First.IsEnabled = false;
        workspace.Update();
        Control tab = NativeContainer(NativeTab(workspace.Manager, workspace.First, tool), tool);
        Assert.False(tab.IsEnabled);
        workspace.Manager.Layout = new();
        workspace.Update();
        Assert.Null(ToolTip.GetTip(tab));
        Assert.True(tab.IsEnabled);
        workspace.First.ToolTip = "Detached model";
        workspace.First.IsEnabled = true;
        workspace.First.IsEnabled = false;
        Assert.Null(ToolTip.GetTip(tab));
        Assert.True(tab.IsEnabled);
    }

    private static Control NativeContainer(Control header, bool tool) => tool
        ? header.FindAncestorOfType<ToolTabStripItem>()! : header.FindAncestorOfType<DocumentTabStripItem>()!;

    private static IImage CreateIcon(IBrush brush) => new DrawingImage
    {
        Drawing = new GeometryDrawing { Brush = brush, Geometry = new RectangleGeometry(new Rect(0, 0, 16, 16)) }
    };

}
