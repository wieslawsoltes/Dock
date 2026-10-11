using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Dock.Model.Avalonia.Core;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>An AvalonDock layout node backed by a live Dock Avalonia model.</summary>
public abstract class LayoutElement : AvaloniaObject, ILayoutElement
{
    private ILayoutContainer? _projectedParent;
    internal LayoutElement(DockableBase model, LayoutModelContext? context = null)
    {
        DockModel = model;
        Context = context ?? new LayoutModelContext();
        Context.Register(this);
        model.PropertyChanged += OnModelPropertyChanged;
    }

    internal LayoutModelContext Context { get; }

    /// <summary>Gets the live Dock model used to render and operate this element.</summary>
    public DockableBase DockModel { get; }

    /// <summary>Gets or sets the containing element.</summary>
    public ILayoutContainer? Parent
    {
        get => _projectedParent ?? (DockModel.Owner is { } owner ? Context.Get(owner) as ILayoutContainer : null);
        set
        {
            if (value is LayoutElement element)
            {
                element.Context.Adopt(Context);
                DockModel.Owner = element.DockModel;
            }
            else if (value is null)
            {
                DockModel.Owner = null;
            }
            else
            {
                throw new ArgumentException("A parent must be a Dock-backed layout element.", nameof(value));
            }
        }
    }

    internal void SetProjectedParent(ILayoutContainer? parent)
    {
        if (ReferenceEquals(_projectedParent, parent)) return;
        LayoutRoot? previousRoot = Root as LayoutRoot;
        RaisePropertyChanging(nameof(Parent));
        _projectedParent = parent;
        previousRoot?.Focus?.Validate();
        RaisePropertyChanged(nameof(Parent));
        RaisePropertyChanged(nameof(Root));
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
    protected void RaisePropertyChanging([CallerMemberName] string? propertyName = null) =>
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

    /// <summary>Raises a property changed notification.</summary>
    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null) =>
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
            if (e.OldValue is DockableBase previousOwner && Context.Get(previousOwner).Root is LayoutRoot previousRoot)
                previousRoot.Focus?.Validate();
            RaisePropertyChanged(nameof(Parent));
            RaisePropertyChanged(nameof(Root));
        }
        else
        {
            RaisePropertyChanged(e.Property.Name);
        }
    }
}
