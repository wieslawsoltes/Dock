// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using BrowserTabTheme;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class BrowserTabThemeMainWindowViewModelTests
{
    [Fact]
    public void ChromeStateTracksAllCurrentDocumentDocks()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        var proportionalDock = factory.CreateProportionalDock();
        proportionalDock.VisibleDockables = factory.CreateList<IDockable>();
        var firstDocumentDock = factory.CreateDocumentDock();
        firstDocumentDock.VisibleDockables = factory.CreateList<IDockable>();

        factory.AddDockable(proportionalDock, firstDocumentDock);
        factory.AddDockable(root, proportionalDock);
        factory.InitLayout(root);

        using var viewModel = new MainWindowViewModel(factory, root);

        Assert.True(viewModel.ExtendClientAreaToDecorationsHint);

        var secondDocumentDock = factory.CreateDocumentDock();
        secondDocumentDock.VisibleDockables = factory.CreateList<IDockable>();
        factory.AddDockable(proportionalDock, secondDocumentDock);
        secondDocumentDock.LayoutMode = DocumentLayoutMode.Mdi;

        Assert.False(viewModel.ExtendClientAreaToDecorationsHint);

        firstDocumentDock.LayoutMode = DocumentLayoutMode.Mdi;
        factory.RemoveDockable(secondDocumentDock, false);

        Assert.False(viewModel.ExtendClientAreaToDecorationsHint);

        firstDocumentDock.LayoutMode = DocumentLayoutMode.Tabbed;

        Assert.True(viewModel.ExtendClientAreaToDecorationsHint);
    }
}
