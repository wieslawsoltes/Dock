using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// The construction boundary for model-bearing public controls. Their visuals and
// native tracking/content composition are provided by compiled control themes.
internal sealed class LayoutGridChildren(Grid grid, ILayoutContainer model)
{
    private readonly List<Control> _desired = new();

    internal void Refresh()
    {
        _desired.Clear();
        if (model.Root?.Manager is null) { Clear(); return; }
        IDock? native = (model as LayoutElement)?.DockModel as IDock;
        foreach (ILayoutElement child in model.Children)
        {
            if (_desired.Count > 0)
            {
                IDockable? splitter = FindSplitter(native, child);
                Control? resizer = FindSplitterControl(splitter);
                resizer ??= (grid.FindResource("AvalonDockGridSplitter") as IDataTemplate)?.Build(splitter);
                if (resizer is not null)
                {
                    resizer.DataContext = splitter;
                    _desired.Add(resizer);
                }
            }
            Control? control = FindControl(child) ?? CreateControl(child);
            if (control is null) continue;
            control.DataContext = ((LayoutElement)child).DockModel;
            control.Classes.Add("avalondock-grid-child");
            _desired.Add(control);
        }
        // Preserve panes on reorder so their native editors and focus survive.
        for (int i = grid.Children.Count - 1; i >= 0; i--)
            if (!_desired.Contains(grid.Children[i])) grid.Children.RemoveAt(i);
        for (int i = 0; i < _desired.Count; i++)
        {
            Control child = _desired[i];
            int current = grid.Children.IndexOf(child);
            if (current < 0) grid.Children.Insert(i, child);
            else if (current != i) grid.Children.Move(current, i);
        }
        _desired.Clear();
    }

    internal void Clear()
    {
        foreach (Control child in grid.Children)
            if (child is ILayoutGridView nested) nested.ReleaseChildren();
        grid.Children.Clear();
        grid.RowDefinitions.Clear();
        grid.ColumnDefinitions.Clear();
        _desired.Clear();
    }

    private Control? FindControl(ILayoutElement model)
    {
        foreach (Control child in grid.Children)
            if (child is ILayoutControl view && ReferenceEquals(view.Model, model)) return child;
        return null;
    }

    private Control? FindSplitterControl(IDockable? model)
    {
        foreach (Control child in grid.Children)
            if (child is LayoutGridResizerControl && ReferenceEquals(child.DataContext, model) && !_desired.Contains(child)) return child;
        return null;
    }

    private static IDockable? FindSplitter(IDock? parent, ILayoutElement next)
    {
        if (parent?.VisibleDockables is not { } children || next is not LayoutElement model) return null;
        int index = children.IndexOf(model.DockModel);
        return index > 0 && children[index - 1] is IProportionalDockSplitter splitter ? splitter : null;
    }

    private static Control? CreateControl(ILayoutElement model) => model switch
    {
        LayoutPanel panel => new LayoutPanelControl(panel),
        LayoutDocumentPaneGroup group => new LayoutDocumentPaneGroupControl(group),
        LayoutAnchorablePaneGroup group => new LayoutAnchorablePaneGroupControl(group),
        LayoutDocumentPane pane => new LayoutDocumentPaneControl(pane),
        LayoutAnchorablePane pane => new LayoutAnchorablePaneControl(pane),
        _ => null
    };
}
