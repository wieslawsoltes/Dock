﻿using System.Diagnostics.CodeAnalysis;
using Dock.Model.Prism.Controls;
using DockPrismSample.ViewModels.Documents;
using Prism.Commands;

namespace DockPrismSample.ViewModels.Docks;

[RequiresUnreferencedCode("Requires unreferenced code for ReactiveCommand.Create.")]
[RequiresDynamicCode("Requires unreferenced code for ReactiveCommand.Create.")]
public class CustomDocumentDock : DocumentDock
{
    public CustomDocumentDock()
    {
        CreateDocument = new DelegateCommand(CreateNewDocument);
    }

    private void CreateNewDocument()
    {
        if (!CanCreateDocument)
            return;

        var index = VisibleDockables?.Count + 1;
        var document = new DocumentViewModel { Id = $"Document{index}", Title = $"Document{index}" };

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }
}
