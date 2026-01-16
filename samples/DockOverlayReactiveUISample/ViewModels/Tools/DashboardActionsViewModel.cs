using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using ReactiveUI;

namespace DockOverlayReactiveUISample.ViewModels.Tools;

[RequiresUnreferencedCode("Requires unreferenced code for ReactiveCommand.")]
[RequiresDynamicCode("Requires dynamic code for ReactiveCommand.")]
public class DashboardActionsViewModel : OverlayToolViewModel
{
    private readonly IOverlayPanel _targetPanel;

    public ICommand FloatPanelCommand { get; }

    public ICommand RestorePanelCommand { get; }

    public DashboardActionsViewModel(IOverlayPanel targetPanel)
    {
        _targetPanel = targetPanel;
        Id = "DashboardActions";
        Title = "Actions";
        CanClose = false;
        Context = this;

        FloatPanelCommand = ReactiveCommand.Create(FloatPanel);
        RestorePanelCommand = ReactiveCommand.Create(RestorePanel);
    }

    private void FloatPanel()
    {
        if (!_targetPanel.CanFloat)
        {
            return;
        }

        if (_targetPanel.Owner is not IOverlayDock)
        {
            return;
        }

        _targetPanel.OriginalOwner ??= _targetPanel.Owner;
        _targetPanel.Factory?.FloatDockable(_targetPanel);
    }

    private void RestorePanel()
    {
        if (_targetPanel.Factory is not { } factory)
        {
            return;
        }

        if (_targetPanel.OriginalOwner is not IDock originalOwner)
        {
            return;
        }

        if (_targetPanel.Owner is not IDock currentOwner)
        {
            return;
        }

        if (ReferenceEquals(originalOwner, currentOwner))
        {
            return;
        }

        factory.MoveDockable(currentOwner, originalOwner, _targetPanel, null);
        _targetPanel.OriginalOwner = null;

        var floatingRoot = factory.FindRoot(currentOwner);
        var originalRoot = factory.FindRoot(originalOwner);

        if (floatingRoot?.Window is { } window
            && !ReferenceEquals(floatingRoot, originalRoot)
            && floatingRoot.IsEmpty)
        {
            factory.RemoveWindow(window);
        }
    }
}
