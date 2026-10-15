using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Dock.Model.Core.Events;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>An AvalonDock layout node backed by a live Dock Avalonia model.</summary>
public abstract class LayoutElement : AvaloniaObject, ILayoutElement
{
    private ILayoutContainer? _projectedParent;
    private bool _retainedCollectionParent;
    private ILayoutRoot? _reportedRoot;
    private int _settingParent;
    private ILayoutContainer? _changingParent;
    private ILayoutRoot? _changingRoot;
    private ILayoutRoot? _changingNativeRoot;
    private bool _hasParentChange;
    private AvaloniaProperty? _xmlRestoreProperty;
    internal LayoutElement(DockableBase model, LayoutModelContext? context = null)
    {
        DockModel = model;
        Context = context ?? new LayoutModelContext();
        Context.Register(this);
        model.OwnerChanging += OwnerChanging;
        model.PropertyChanged += ModelPropertyChanged;
    }

    internal LayoutModelContext Context { get; }

    // Projections follow the completed parent write even when a public hook
    // aborts its remaining notifications. This is independent of virtual dispatch.
    internal event Action? ParentChangeCompleted;

    // Upstream group XML writes backing fields. Native Dock still needs its
    // property notification, while the public facade must remain silent.
    internal void RestoreXmlProperty<T>(DirectPropertyBase<T> property, T value)
    {
        VerifyAccess();
        AvaloniaProperty? previous = _xmlRestoreProperty;
        _xmlRestoreProperty = property;
        try { DockModel.SetValue(property, value); }
        finally { _xmlRestoreProperty = previous; }
    }

    private void ModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (ReferenceEquals(_xmlRestoreProperty, e.Property))
        {
            // Consume only this notification. Reentrant public changes from a
            // native observer retain their ordinary compatibility callbacks.
            _xmlRestoreProperty = null;
            return;
        }
        OnModelPropertyChanged(sender, e);
    }

    // Dock's unset Id is empty; WPF's serialized pane Id starts at null.
    // Preserve an explicitly read empty attribute without changing native Ids.
    internal bool HasExplicitXmlId { get; set; }
    internal string? XmlId => HasExplicitXmlId || !string.IsNullOrEmpty(DockModel.Id) ? DockModel.Id : null;

    /// <summary>Gets the live Dock model used to render and operate this element.</summary>
    public DockableBase DockModel { get; }

    /// <summary>Gets or sets the containing element.</summary>
    /// <remarks>Application-defined containers are supported without native ownership.
    /// Root callbacks compare the root recorded after the last completed parent hook.</remarks>
    public ILayoutContainer? Parent
    {
        get => _projectedParent ?? GetNativeParent(DockModel.Owner);
        set
        {
            VerifyAccess();
            ILayoutContainer? previous = Parent;
            if (ReferenceEquals(previous, value)) return;
            ILayoutRoot? previousRoot = _reportedRoot;
            ILayoutRoot? previousNativeRoot = Root;
            if (value is LayoutElement element) element.Context.Adopt(Context);
            NotifyParentChanging(previous, value);
            _settingParent++;
            try
            {
                _retainedCollectionParent = false;
                _projectedParent = value is LayoutElement ? null : value;
                DockModel.Owner = (value as LayoutElement)?.DockModel;
            }
            finally { _settingParent--; }
            NotifyParentChanged(previous, Parent, previousRoot, previousNativeRoot);
        }
    }

    internal void SetProjectedParent(ILayoutContainer? parent)
    {
        _retainedCollectionParent = false;
        if (ReferenceEquals(_projectedParent, parent)) return;
        ILayoutContainer? previous = Parent;
        ILayoutContainer? next = parent ?? GetNativeParent(DockModel.Owner);
        if (ReferenceEquals(previous, next)) { _projectedParent = parent; return; }
        ILayoutRoot? previousRoot = _reportedRoot;
        ILayoutRoot? previousNativeRoot = Root;
        NotifyParentChanging(previous, next);
        _projectedParent = parent;
        NotifyParentChanged(previous, Parent, previousRoot, previousNativeRoot);
    }

    // ObservableCollection.Reset has no old items in AvalonDock's group
    // callback. Keep that public parent without leaving a native owner behind.
    internal void RetainParentAfterCollectionReset()
    {
        _projectedParent = Parent;
        _retainedCollectionParent = _projectedParent is not null;
    }

    // Single-document floating windows expose the document directly. Their
    // native DocumentDock is an implementation detail, including during moves.
    private bool IsHiddenNativeParent(IDockable? owner)
    {
        if (this is not LayoutDocument || owner is not DocumentDock { Owner: RootDock { Window: not null } root } pane)
            return false;
        if (Context.FindExisting(root) is LayoutFloatingWindow window) return window is LayoutDocumentFloatingWindow;
        return root.VisibleDockables is { Count: 1 } children && ReferenceEquals(children[0], pane) &&
            pane.VisibleDockables is { Count: 1 } contents && contents[0] is Document;
    }

    private ILayoutContainer? GetNativeParent(IDockable? owner) =>
        owner is null || IsHiddenNativeParent(owner) ? null : Context.Get(owner) as ILayoutContainer;

    private void OwnerChanging(object? sender, DockableOwnerChangingEventArgs args)
    {
        if (_settingParent != 0) return;
        bool adoptingFloatingChild = _projectedParent is LayoutFloatingWindow floating &&
            args.NewOwner is { } owner && !floating.OwnsNativeChild(owner);
        bool adoptingResetChild = _retainedCollectionParent && args.NewOwner is not null;
        if (_projectedParent is not null && !adoptingFloatingChild && !adoptingResetChild) return;
        ILayoutContainer? previous = Parent;
        ILayoutRoot? previousRoot = _reportedRoot;
        ILayoutRoot? previousNativeRoot = Root;
        if (!IsHiddenNativeParent(args.NewOwner) && args.NewOwner?.Factory is LayoutFactory factory &&
            factory.GetLayoutContainer(args.NewOwner) is LayoutElement parent)
            parent.Context.Adopt(Context);
        ILayoutContainer? next = GetNativeParent(args.NewOwner);
        if (ReferenceEquals(previous, next)) return;
        _changingParent = previous;
        _changingRoot = previousRoot;
        _changingNativeRoot = previousNativeRoot;
        _hasParentChange = true;
        NotifyParentChanging(previous, next);
        if (adoptingFloatingChild || adoptingResetChild)
        {
            _projectedParent = null;
            _retainedCollectionParent = false;
        }
    }

    private void NotifyParentChanging(ILayoutContainer? previous, ILayoutContainer? next)
    {
        RaisePropertyChanging(nameof(Parent));
        OnParentChanging(previous, next);
    }

    private void NotifyParentChanged(ILayoutContainer? previous, ILayoutContainer? next, ILayoutRoot? previousRoot, ILayoutRoot? previousNativeRoot)
    {
        ClearParentChange();
        try
        {
            OnParentChanged(previous, next);
            _reportedRoot = Root;
            if (!ReferenceEquals(previousRoot, _reportedRoot)) OnRootChanged(previousRoot, _reportedRoot);
            (previousNativeRoot as LayoutRoot)?.Focus?.Validate();
            RaisePropertyChanged(nameof(Parent));
            (Root as LayoutRoot)?.NotifyUpdated();
        }
        finally
        {
            ParentChangeCompleted?.Invoke();
        }
    }

    private void ClearParentChange()
    {
        _hasParentChange = false;
        _changingParent = null;
        _changingRoot = null;
        _changingNativeRoot = null;
    }

    /// <summary>Called before a parent transition, including native ownership changes.</summary>
    protected virtual void OnParentChanging(ILayoutContainer? oldValue, ILayoutContainer? newValue) { }

    /// <summary>Called after the public parent and native ownership change.</summary>
    protected virtual void OnParentChanged(ILayoutContainer? oldValue, ILayoutContainer? newValue) { }

    /// <summary>Notifies the previous and new layout roots about this element's membership.</summary>
    protected virtual void OnRootChanged(ILayoutRoot? oldRoot, ILayoutRoot? newRoot)
    {
        if (oldRoot is not null) ((LayoutRoot)oldRoot).NotifyElementRemoved(this);
        if (newRoot is not null) ((LayoutRoot)newRoot).NotifyElementAdded(this);
    }

    /// <inheritdoc/>
    /// <remarks>Traversal starts at the parent; an unparented layout root has no owning root.</remarks>
    public ILayoutRoot? Root
    {
        get
        {
            ILayoutContainer? current = Parent;
            while (current is not null)
            {
                if (current is ILayoutRoot result) return result;
                current = current.Parent;
            }
            return null;
        }
    }

    /// <inheritdoc/>
    public new event PropertyChangedEventHandler? PropertyChanged;
    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Raises a property changing notification.</summary>
    protected virtual void RaisePropertyChanging([CallerMemberName] string? propertyName = null) =>
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

    /// <summary>Raises a property changed notification.</summary>
    protected virtual void RaisePropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    internal bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        RaisePropertyChanging(name);
        field = value;
        RaisePropertyChanged(name);
        return true;
    }

    internal bool SetField(ref double field, double value, [CallerMemberName] string? name = null)
    {
        if (field == value) return false;
        RaisePropertyChanging(name);
        field = value;
        RaisePropertyChanged(name);
        return true;
    }

    internal bool SetFieldChanged<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        VerifyAccess();
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        RaisePropertyChanged(name);
        return true;
    }

    internal void SetReferenceChanged<T>(ref T? field, T? value, [CallerMemberName] string? name = null) where T : class
    {
        VerifyAccess();
        if (ReferenceEquals(field, value)) return;
        field = value;
        RaisePropertyChanged(name);
    }

    internal virtual void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == DockableBase.OwnerProperty)
        {
            if (_retainedCollectionParent && e.NewValue is IDockable owner && ReferenceEquals(Parent, GetNativeParent(owner)))
            {
                _projectedParent = null;
                _retainedCollectionParent = false;
                return;
            }
            if (_settingParent != 0 || _projectedParent is not null) return;
            ILayoutContainer? previous = _hasParentChange ? _changingParent : GetNativeParent(e.OldValue as IDockable);
            ILayoutRoot? previousRoot = _hasParentChange ? _changingRoot : _reportedRoot;
            ILayoutRoot? previousNativeRoot = _hasParentChange ? _changingNativeRoot : previous as ILayoutRoot ?? previous?.Root;
            ClearParentChange();
            if (!ReferenceEquals(previous, Parent)) NotifyParentChanged(previous, Parent, previousRoot, previousNativeRoot);
        }
        else
        {
            RaisePropertyChanged(e.Property.Name);
        }
    }
}
