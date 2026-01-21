using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Model.Services;

namespace Dock.Avalonia.Behaviors;

/// <summary>
/// Invokes <see cref="IVisualTreeLifecycle"/> hooks for attached elements and their data contexts.
/// </summary>
public sealed class VisualTreeLifecycleBehavior
{
    /// <summary>
    /// Defines the IsEnabled attached property.
    /// </summary>
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<VisualTreeLifecycleBehavior, Control, bool>("IsEnabled");

    static VisualTreeLifecycleBehavior()
    {
        IsEnabledProperty.Changed.AddClassHandler<Control>(OnIsEnabledChanged);
        Control.DataContextProperty.Changed.AddClassHandler<Control>(OnDataContextChanged);
    }

    /// <summary>
    /// Gets the IsEnabled attached property value.
    /// </summary>
    /// <param name="element">The target element.</param>
    /// <returns>The current value.</returns>
    public static bool GetIsEnabled(Control element)
        => element.GetValue(IsEnabledProperty);

    /// <summary>
    /// Sets the IsEnabled attached property value.
    /// </summary>
    /// <param name="element">The target element.</param>
    /// <param name="value">The new value.</param>
    public static void SetIsEnabled(Control element, bool value)
        => element.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(Control element, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not bool isEnabled)
        {
            return;
        }

        if (isEnabled)
        {
            element.AttachedToVisualTree += OnAttachedToVisualTree;
            element.DetachedFromVisualTree += OnDetachedFromVisualTree;

            if (element.IsAttachedToVisualTree())
            {
                InvokeAttached(element);
            }
        }
        else
        {
            element.AttachedToVisualTree -= OnAttachedToVisualTree;
            element.DetachedFromVisualTree -= OnDetachedFromVisualTree;

            if (element.IsAttachedToVisualTree())
            {
                InvokeDetached(element);
            }
        }
    }

    private static void OnDataContextChanged(Control element, AvaloniaPropertyChangedEventArgs e)
    {
        if (!GetIsEnabled(element) || !element.IsAttachedToVisualTree())
        {
            return;
        }

        if (e.OldValue is IVisualTreeLifecycle oldLifecycle)
        {
            oldLifecycle.OnDetachedFromVisualTree();
        }

        if (e.NewValue is IVisualTreeLifecycle newLifecycle)
        {
            newLifecycle.OnAttachedToVisualTree();
        }
    }

    private static void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control element)
        {
            InvokeAttached(element);
        }
    }

    private static void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control element)
        {
            InvokeDetached(element);
        }
    }

    private static void InvokeAttached(Control element)
    {
        if (element is IVisualTreeLifecycle lifecycle)
        {
            lifecycle.OnAttachedToVisualTree();
        }

        if (element.DataContext is IVisualTreeLifecycle dataContext
            && !ReferenceEquals(dataContext, element))
        {
            dataContext.OnAttachedToVisualTree();
        }
    }

    private static void InvokeDetached(Control element)
    {
        if (element.DataContext is IVisualTreeLifecycle dataContext
            && !ReferenceEquals(dataContext, element))
        {
            dataContext.OnDetachedFromVisualTree();
        }

        if (element is IVisualTreeLifecycle lifecycle)
        {
            lifecycle.OnDetachedFromVisualTree();
        }
    }
}
