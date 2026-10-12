using Dock.Model.Avalonia.Controls;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>A group of tools in an auto-hide strip.</summary>
public class LayoutAnchorGroup : LayoutGroup<LayoutAnchorable>
{
    private ILayoutContainer? _previousContainer;

    /// <summary>Creates an empty auto-hide group.</summary>
    public LayoutAnchorGroup() : base(new ToolDock()) { }

    internal ILayoutContainer? PreviousContainer
    {
        get => _previousContainer;
        set
        {
            VerifyAccess();
            if (ReferenceEquals(_previousContainer, value)) return;
            _previousContainer = value;
            RaisePropertyChanged();
            LayoutContainerIdentity.EnsureReferencedId(_previousContainer);
        }
    }
    internal string? PreviousContainerId { get; set; }

    /// <inheritdoc/>
    protected override bool GetVisibility() => Children.Count > 0;
}
