// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Avalonia.Automation.Peers;
using Dock.Model.Controls;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="RootDockControl"/> xaml.
/// </summary>
[TemplatePart("PART_MainContent", typeof(ContentControl)/*, IsRequired = true*/)]
public class RootDockControl : TemplatedControl
{
    private ContentControl? _mainContent;

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new RootDockControlAutomationPeer(this);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_mainContent is not null)
        {
            _mainContent.RemoveHandler(PointerPressedEvent, MainContentPointerPressedHandler);
        }

        _mainContent = e.NameScope.Find<ContentControl>("PART_MainContent");
        if (_mainContent is not null)
        {
            _mainContent.AddHandler(PointerPressedEvent, MainContentPointerPressedHandler, RoutingStrategies.Tunnel);
        }
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_mainContent is not null)
        {
            _mainContent.RemoveHandler(PointerPressedEvent, MainContentPointerPressedHandler);
            _mainContent = null;
        }
    }

    private void MainContentPointerPressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is IRootDock rootDock)
        {
            rootDock.Factory?.HidePreviewingDockables(rootDock);
        }
    }
}
