using System;
using System.Reactive.Linq;
using Dock.Model.ReactiveUI.Navigation.Controls;
using DockFigmaSample.ViewModels;
using ReactiveUI;

namespace DockFigmaSample.ViewModels.Tools;

public class InspectorToolViewModel : RoutableTool
{
    private readonly InspectorDesignViewModel _designView;
    private readonly InspectorPrototypeViewModel _prototypeView;
    private readonly InspectorInspectViewModel _inspectView;

    public InspectorToolViewModel(IScreen host) : base(host, "inspector")
    {
        _designView = new InspectorDesignViewModel(this);
        _prototypeView = new InspectorPrototypeViewModel(this);
        _inspectView = new InspectorInspectViewModel(this);

        Router.Navigate.Execute(_designView).Subscribe(_ => { });
    }

    public void SetMode(WorkspaceMode mode)
    {
        switch (mode)
        {
            case WorkspaceMode.Design:
                Router.Navigate.Execute(_designView).Subscribe(_ => { });
                break;
            case WorkspaceMode.Prototype:
                Router.Navigate.Execute(_prototypeView).Subscribe(_ => { });
                break;
            case WorkspaceMode.Inspect:
                Router.Navigate.Execute(_inspectView).Subscribe(_ => { });
                break;
        }
    }
}
