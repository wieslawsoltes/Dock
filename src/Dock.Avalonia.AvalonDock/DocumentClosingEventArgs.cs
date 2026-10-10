using System.ComponentModel;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock;

/// <summary>Describes a document close request that may be canceled.</summary>
public class DocumentClosingEventArgs : CancelEventArgs
{
    /// <summary>Creates event data for a document.</summary>
    public DocumentClosingEventArgs(LayoutDocument document) => Document = document;
    /// <summary>Gets the document being closed.</summary>
    public LayoutDocument Document { get; }
}
