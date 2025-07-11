using System.IO;
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

    public MainWindowViewModel(IFactory factory, IDockSerializer serializer, IDockState state)
    {
        _factory = factory;
        _serializer = serializer;
        _state = state;
        LoadLayout();
    }

    private void LoadLayout()
    {
        const string path = "layout.json";
        if (false && File.Exists(path))
        {
            using var stream = File.OpenRead(path);
            var layout = _serializer.Load<IRootDock?>(stream);
            if (layout is { })
            {
                _factory.InitLayout(layout);
                _state.Restore(layout);
                Layout = layout;
                return;
            }
        }
        Layout = _factory.CreateLayout();
        if (Layout is { })
        {
            _factory.InitLayout(Layout);
        }
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
