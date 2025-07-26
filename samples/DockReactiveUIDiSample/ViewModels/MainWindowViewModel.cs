using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Dock.Model.Controls;
using Dock.Model.Core;
using ReactiveUI;

namespace DockReactiveUIDiSample.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly IFactory _factory;
    private readonly IDockSerializer _serializer;
    private readonly IDockState _state;

    private IRootDock? _layout;

    public IRootDock? Layout
    {
        get => _layout;
        set => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    public ReactiveCommand<Unit, Unit> LoadLayoutCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveLayoutCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseLayoutCommand { get; }

    public MainWindowViewModel(IFactory factory, IDockSerializer serializer, IDockState state)
    {
        _factory = factory;
        _serializer = serializer;
        _state = state;

        LoadLayoutCommand = ReactiveCommand.Create(LoadLayout);
        SaveLayoutCommand = ReactiveCommand.CreateFromTask(SaveLayoutAsync);
        CloseLayoutCommand = ReactiveCommand.Create(CloseLayout);

        LoadLayout();
    }

    public void LoadLayout()
    {
        const string path = "layout.json";
        if (File.Exists(path))
        {
            using var stream = File.OpenRead(path);
            var rootDockLayout = _serializer.Load<IRootDock?>(stream);
            if (rootDockLayout is { })
            {
                _factory.InitLayout(rootDockLayout);
                _state.Restore(rootDockLayout);
                Layout = rootDockLayout;
                return;
            }
        }
        var layout = _factory.CreateLayout();
        if (layout is not null)
        {
            _factory.InitLayout(layout);
        }
        Layout = layout;
    }

    public async Task SaveLayoutAsync()
    {
        if (Layout is not { })
            return;

        _state.Save(Layout);
        const string path = "layout.json";
        await using var stream = File.Create(path);
        _serializer.Save(stream, Layout);
    }

    public void CloseLayout()
    {
        if (Layout is IDock dock && dock.Close.CanExecute(null))
        {
            dock.Close.Execute(null);
        }
    }
}
