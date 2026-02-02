using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using ReactiveUI;

namespace DockReactiveUIWindowRelationsSample.ViewModels;

[RequiresUnreferencedCode("Requires unreferenced code for RaiseAndSetIfChanged.")]
[RequiresDynamicCode("Requires unreferenced code for RaiseAndSetIfChanged.")]
public class MainWindowViewModel : ReactiveObject
{
    private readonly IFactory? _factory;
    private IRootDock? _layout;

    public IRootDock? Layout
    {
        get => _layout;
        set => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    public ICommand ResetLayout { get; }

    public MainWindowViewModel()
    {
        _factory = new DockFactory();
        Layout = CreateLayout();
        ResetLayout = ReactiveCommand.Create(Reset);
    }

    private IRootDock? CreateLayout()
    {
        var layout = _factory?.CreateLayout();
        if (layout is not null)
        {
            _factory?.InitLayout(layout);
        }

        return layout;
    }

    public void CloseLayout()
    {
        if (Layout is IDock dock && dock.Close.CanExecute(null))
        {
            dock.Close.Execute(null);
        }
    }

    private void Reset()
    {
        CloseLayout();
        Layout = CreateLayout();
    }
}
