using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Dock.Model.Controls;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="RootDockControl"/> xaml.
/// </summary>
[TemplatePart("PART_MainContent", typeof(ContentControl)/*, IsRequired = true*/)]
public class RootDockControl : TemplatedControl
{
    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var mainContent = e.NameScope.Get<ContentControl>("PART_MainContent");
        mainContent.AddHandler(PointerPressedEvent, (_, _) =>
        {
            if (DataContext is IRootDock rootDock)
                rootDock.Factory?.HidePreviewingDockables(rootDock);
        }, RoutingStrategies.Tunnel);
    }
}
