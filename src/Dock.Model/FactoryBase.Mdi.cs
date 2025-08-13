// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

public abstract partial class FactoryBase
{
    /// <inheritdoc/>
    public virtual void SetDocumentDockPresentation(IDockable dockable, DocumentPresentation presentation)
    {
        if (dockable is IDocumentDock doc)
        {
            doc.Presentation = presentation;
            if (presentation == DocumentPresentation.Mdi)
            {
                // Provide an initial, usable layout when switching to MDI
                CascadeDocuments(doc);
            }
        }
    }

    /// <inheritdoc/>
    public virtual void SetDocumentDockPresentationMdi(IDockable dockable) => SetDocumentDockPresentation(dockable, DocumentPresentation.Mdi);

    /// <inheritdoc/>
    public virtual void SetDocumentDockPresentationTabs(IDockable dockable) => SetDocumentDockPresentation(dockable, DocumentPresentation.Tabs);

    /// <inheritdoc/>
    public virtual void CascadeDocuments(IDocumentDock documentDock)
    {
        if (documentDock.VisibleDockables is null || documentDock.VisibleDockables.Count == 0)
            return;

        const double baseWidth = 640;
        const double baseHeight = 480;
        const double offsetStep = 28;

        var index = 0;
        foreach (var dockable in documentDock.VisibleDockables)
        {
            if (dockable is not IDocument)
                continue;

            var x = 20 + index * offsetStep;
            var y = 20 + index * offsetStep;
            var width = baseWidth;
            var height = baseHeight;

            dockable.SetVisibleBounds(x, y, width, height);
            index++;
        }
    }

    /// <inheritdoc/>
    public virtual void TileDocumentsHorizontally(IDocumentDock documentDock)
    {
        if (documentDock.VisibleDockables is null)
            return;

        // Determine number of documents
        var documents = documentDock.VisibleDockables;
        var count = 0;
        foreach (var d in documents)
            if (d is IDocument) count++;
        if (count == 0) return;

        // Default client size if actual size cannot be determined from view
        const double clientWidth = 1000;
        const double clientHeight = 700;

        var tileHeight = clientHeight / count;
        var y = 0.0;
        foreach (var dockable in documents)
        {
            if (dockable is not IDocument) continue;
            dockable.SetVisibleBounds(0, y, clientWidth, tileHeight);
            y += tileHeight;
        }
    }

    /// <inheritdoc/>
    public virtual void TileDocumentsVertically(IDocumentDock documentDock)
    {
        if (documentDock.VisibleDockables is null)
            return;

        var documents = documentDock.VisibleDockables;
        var count = 0;
        foreach (var d in documents)
            if (d is IDocument) count++;
        if (count == 0) return;

        const double clientWidth = 1000;
        const double clientHeight = 700;

        var tileWidth = clientWidth / count;
        var x = 0.0;
        foreach (var dockable in documents)
        {
            if (dockable is not IDocument) continue;
            dockable.SetVisibleBounds(x, 0, tileWidth, clientHeight);
            x += tileWidth;
        }
    }
}


