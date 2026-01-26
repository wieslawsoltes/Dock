using System;
using System.Reactive.Linq;
using Dock.Model.ReactiveUI.Navigation.Controls;
using DockFigmaSample.ViewModels;
using ReactiveUI;

namespace DockFigmaSample.ViewModels.Documents;

public class CanvasDocumentViewModel : RoutableDocument
{
    private readonly DesignCanvasViewModel _designView;
    private readonly PrototypeCanvasViewModel _prototypeView;
    private readonly InspectCanvasViewModel _inspectView;

    public CanvasDocumentViewModel(IScreen host) : base(host, "canvas")
    {
        _designView = new DesignCanvasViewModel(this);
        _prototypeView = new PrototypeCanvasViewModel(this);
        _inspectView = new InspectCanvasViewModel(this);

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
