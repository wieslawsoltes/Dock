using System.ComponentModel;

namespace AvalonDock.Layout.Serialization;

/// <summary>Supplies application content while restoring a layout.</summary>
public class LayoutSerializationCallbackEventArgs : CancelEventArgs
{
    /// <summary>Creates callback data with content from the previous layout.</summary>
    public LayoutSerializationCallbackEventArgs(LayoutContent model, object? previousContent)
    {
        Model = model;
        Content = previousContent;
    }
    /// <summary>Gets the restored layout model.</summary>
    public LayoutContent Model { get; }
    /// <summary>Gets or sets the application content to attach.</summary>
    public object? Content { get; set; }
}
