using System;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock;

/// <summary>Describes a document that has closed.</summary>
public class DocumentClosedEventArgs : EventArgs
{
    /// <summary>Creates event data for a document.</summary>
    public DocumentClosedEventArgs(LayoutDocument document) => Document = document;
    /// <summary>Gets the closed document.</summary>
    public LayoutDocument Document { get; }
}
