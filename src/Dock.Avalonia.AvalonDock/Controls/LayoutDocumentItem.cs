using Avalonia;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>A bindable container for a layout document.</summary>
public class LayoutDocumentItem : LayoutItem
{
    /// <summary>Identifies the document description property.</summary>
    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<LayoutDocumentItem, string?>(nameof(Description));

    /// <summary>Gets or sets the document description.</summary>
    public string? Description { get => GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }

    /// <inheritdoc/>
    protected override void Close() => LayoutElement?.Close();

    /// <summary>Forwards a change to the document description.</summary>
    protected virtual void OnDescriptionChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!IsSynchronizing && LayoutElement is LayoutDocument document) document.Description = Description;
    }

    /// <inheritdoc/>
    protected override void OnVisibilityChanged()
    {
        base.OnVisibilityChanged();
        SynchronizeModel();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DescriptionProperty) OnDescriptionChanged(change);
    }

    internal override void UpdateModelProperties()
    {
        base.UpdateModelProperties();
        if (LayoutElement is not LayoutDocument document) return;
        SetCurrentValue(DescriptionProperty, document.Description);
        SetCurrentValue(IsVisibleProperty, document.IsVisible);
    }
}
