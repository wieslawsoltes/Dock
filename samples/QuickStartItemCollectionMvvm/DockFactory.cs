using System;
using Dock.Model.Avalonia;
using Dock.Model.Core;

namespace QuickStartMultiDocumentMvvm;

/// <summary>
/// Allow us to manage the document collection 
/// </summary>
public class DockFactory(Action createDocument) : Factory
{
    /// <summary>
    /// You could use this to do any last-minute work or change your mind
    /// - the framework will remove the FileDocument from the collection
    /// </summary>
    public override bool OnDockableClosing(IDockable? dockable) => true;


    /// <summary>
    /// Called when the + button is pressed. Return true to indicate that the DockFactory is managing its own collection
    /// </summary>
    public override bool AddDocumentToBoundCollection()
    {
        createDocument();
        return true;
    }
}
