using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Layout;

namespace Xceed.Wpf.AvalonDock.Layout;

/// <summary>A node in an AvalonDock layout.</summary>
public interface ILayoutElement : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <summary>Gets the containing node.</summary>
    ILayoutContainer? Parent { get; }
    /// <summary>Gets the owning layout.</summary>
    ILayoutRoot? Root { get; }
}

/// <summary>A node containing other layout elements.</summary>
public interface ILayoutContainer : ILayoutElement
{
    /// <summary>Gets the immediate children.</summary>
    IEnumerable<ILayoutElement> Children { get; }
    /// <summary>Gets the number of children.</summary>
    int ChildrenCount { get; }
    /// <summary>Removes a child.</summary>
    void RemoveChild(ILayoutElement element);
    /// <summary>Replaces a child.</summary>
    void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement);
}

/// <summary>An indexed group of layout elements.</summary>
public interface ILayoutGroup : ILayoutContainer
{
    /// <summary>Finds a child's index.</summary>
    int IndexOfChild(ILayoutElement element);
    /// <summary>Inserts a child.</summary>
    void InsertChildAt(int index, ILayoutElement element);
    /// <summary>Removes the child at an index.</summary>
    void RemoveChildAt(int index);
    /// <summary>Replaces the child at an index.</summary>
    void ReplaceChildAt(int index, ILayoutElement element);
    /// <summary>Occurs when immediate children change.</summary>
    event EventHandler? ChildrenCollectionChanged;
}

/// <summary>A group with a split direction.</summary>
public interface ILayoutOrientableGroup : ILayoutGroup
{
    /// <summary>Gets or sets the split direction.</summary>
    Orientation Orientation { get; set; }
}

/// <summary>An element that can participate in a layout panel.</summary>
public interface ILayoutPanelElement : ILayoutElement
{
    /// <summary>Gets whether the element has visible content.</summary>
    bool IsVisible { get; }
}

/// <summary>An element with computed visibility.</summary>
public interface ILayoutElementWithVisibility
{
    /// <summary>Recomputes visibility.</summary>
    void ComputeVisibility();
}

/// <summary>A reorderable pane.</summary>
public interface ILayoutPane : ILayoutContainer, ILayoutElementWithVisibility
{
    /// <summary>Moves a child to a different index.</summary>
    void MoveChild(int oldIndex, int newIndex);
    /// <summary>Removes the child at an index.</summary>
    void RemoveChildAt(int childIndex);
}

/// <summary>A document pane or document pane group.</summary>
public interface ILayoutDocumentPane : ILayoutPanelElement, ILayoutPane { }

/// <summary>An anchorable pane or anchorable pane group.</summary>
public interface ILayoutAnchorablePane : ILayoutPanelElement, ILayoutPane { }

/// <summary>A pane with a selected content item.</summary>
public interface ILayoutContentSelector
{
    /// <summary>Gets or sets the selected index, or -1.</summary>
    int SelectedContentIndex { get; set; }
    /// <summary>Gets the selected item.</summary>
    LayoutContent? SelectedContent { get; }
    /// <summary>Finds a content item's index.</summary>
    int IndexOf(LayoutContent content);
}

/// <summary>Customizes insertion of items from a manager's sources.</summary>
public interface ILayoutUpdateStrategy
{
    /// <summary>Returns true if the strategy inserted the anchorable.</summary>
    bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer);
    /// <summary>Called after inserting an anchorable.</summary>
    void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown);
    /// <summary>Returns true if the strategy inserted the document.</summary>
    bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer);
    /// <summary>Called after inserting a document.</summary>
    void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown);
}
