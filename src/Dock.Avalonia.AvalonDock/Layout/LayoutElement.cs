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
    private int _settingParent;
    private ILayoutContainer? _changingParent;
    private ILayoutRoot? _changingRoot;
    private bool _hasParentChange;
    internal LayoutElement(DockableBase model, LayoutModelContext? context = null)
    {
        DockModel = model;
        Context = context ?? new LayoutModelContext();
        Context.Register(this);
        model.OwnerChanging += OwnerChanging;
        model.PropertyChanged += OnModelPropertyChanged;
    }

    internal LayoutModelContext Context { get; }

    /// <summary>Gets the live Dock model used to render and operate this element.</summary>
    public DockableBase DockModel { get; }

    /// <summary>Gets or sets the containing element.</summary>
    public ILayoutContainer? Parent
    {
        get => _projectedParent ?? GetNativeParent(DockModel.Owner);
        set
        {
            if (value is not null and not LayoutElement)
                throw new ArgumentException("A parent must be a Dock-backed layout element.", nameof(value));
            ILayoutContainer? previous = Parent;
            if (ReferenceEquals(previous, value)) return;
            ILayoutRoot? previousRoot = Root;
            if (value is LayoutElement element) element.Context.Adopt(Context);
            NotifyParentChanging(previous, value);
            _settingParent++;
            try
            {
                _projectedParent = null;
                DockModel.Owner = (value as LayoutElement)?.DockModel;
            }
            finally { _settingParent--; }
            NotifyParentChanged(previous, Parent, previousRoot);
        }
    }

    internal void SetProjectedParent(ILayoutContainer? parent)
    {
        if (ReferenceEquals(_projectedParent, parent)) return;
        ILayoutContainer? previous = Parent;
        ILayoutContainer? next = parent ?? GetNativeParent(DockModel.Owner);
        if (ReferenceEquals(previous, next)) { _projectedParent = parent; return; }
        ILayoutRoot? previousRoot = Root;
        NotifyParentChanging(previous, next);
        _projectedParent = parent;
        NotifyParentChanged(previous, Parent, previousRoot);
    }

    // Single-document floating windows expose the document directly. Their
    // native DocumentDock is an implementation detail, including during moves.
    private bool IsHiddenNativeParent(IDockable? owner) =>
        this is LayoutDocument && owner is DocumentDock { Owner: RootDock { Window: not null } };

    private ILayoutContainer? GetNativeParent(IDockable? owner) =>
        owner is null || IsHiddenNativeParent(owner) ? null : Context.Get(owner) as ILayoutContainer;

    private void OwnerChanging(object? sender, DockableOwnerChangingEventArgs args)
    {
        if (_settingParent != 0 || _projectedParent is not null) return;
        ILayoutContainer? previous = Parent;
        ILayoutRoot? previousRoot = Root;
        if (!IsHiddenNativeParent(args.NewOwner) && args.NewOwner?.Factory is LayoutFactory factory &&
            factory.GetLayoutContainer(args.NewOwner) is LayoutElement parent)
            parent.Context.Adopt(Context);
        ILayoutContainer? next = GetNativeParent(args.NewOwner);
        if (ReferenceEquals(previous, next)) return;
        _changingParent = previous;
        _changingRoot = previousRoot;
        _hasParentChange = true;
        NotifyParentChanging(previous, next);
    }

    private void NotifyParentChanging(ILayoutContainer? previous, ILayoutContainer? next)
    {
        RaisePropertyChanging(nameof(Parent));
        OnParentChanging(previous, next);
    }

    private void NotifyParentChanged(ILayoutContainer? previous, ILayoutContainer? next, ILayoutRoot? previousRoot)
    {
        ClearParentChange();
        OnParentChanged(previous, next);
        ILayoutRoot? root = Root;
        if (!ReferenceEquals(previousRoot, root)) OnRootChanged(previousRoot, root);
        (previousRoot as LayoutRoot)?.Focus?.Validate();
        RaisePropertyChanged(nameof(Parent));
        RaisePropertyChanged(nameof(Root));
        (root as LayoutRoot)?.NotifyUpdated();
    }

    private void ClearParentChange()
    {
        _hasParentChange = false;
        _changingParent = null;
        _changingRoot = null;
    }

    /// <summary>Called before a parent transition, including native ownership changes.</summary>
    protected virtual void OnParentChanging(ILayoutContainer? oldValue, ILayoutContainer? newValue) { }

    /// <summary>Called after the public parent and native ownership change.</summary>
    protected virtual void OnParentChanged(ILayoutContainer? oldValue, ILayoutContainer? newValue) { }

    /// <summary>Notifies the previous and new layout roots about this element's membership.</summary>
    protected virtual void OnRootChanged(ILayoutRoot? oldRoot, ILayoutRoot? newRoot)
    {
        (oldRoot as LayoutRoot)?.NotifyElementRemoved(this);
        (newRoot as LayoutRoot)?.NotifyElementAdded(this);
    }

    /// <inheritdoc/>
    public ILayoutRoot? Root
    {
        get
        {
            if (this is ILayoutRoot root) return root;
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

    internal virtual void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == DockableBase.OwnerProperty)
        {
            if (_settingParent != 0 || _projectedParent is not null) return;
            ILayoutContainer? previous = _hasParentChange ? _changingParent : GetNativeParent(e.OldValue as IDockable);
            ILayoutRoot? previousRoot = _hasParentChange ? _changingRoot : previous?.Root;
            ClearParentChange();
            if (!ReferenceEquals(previous, Parent)) NotifyParentChanged(previous, Parent, previousRoot);
        }
        else
        {
            RaisePropertyChanged(e.Property.Name);
        }
    }
}
