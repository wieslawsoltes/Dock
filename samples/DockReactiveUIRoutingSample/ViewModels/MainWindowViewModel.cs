using System;
using System.Reactive.Linq;
using ReactiveUI;

namespace DockReactiveUIRoutingSample.ViewModels;

public class MainWindowViewModel : ReactiveObject, IScreen
{
    public RoutingState Router { get; } = new RoutingState();
    
    private readonly ObservableAsPropertyHelper<bool> _canNavigateBack;
    public bool CanNavigateBack => _canNavigateBack.Value;

    public MainWindowViewModel()
    {
        // Navigate to the dock view on startup
        var dockViewModel = new DockViewModel(this);
        Router.Navigate.Execute(dockViewModel).Subscribe();
        
        // Set up the CanNavigateBack property
        _canNavigateBack = this.WhenAnyValue(x => x.Router.NavigationStack.Count)
            .Select(count => count > 1)
            .ToProperty(this, x => x.CanNavigateBack);
    }
}
