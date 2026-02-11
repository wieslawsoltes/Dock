using Dock.Model.Core;
using ReactiveUI;

namespace DockXamlReactiveUISample.ViewModels;

public class CapabilityPolicyEditor : ReactiveObject
{
    private readonly DockCapabilityPolicy _policy;
    private bool? _canClose;
    private bool? _canPin;
    private bool? _canFloat;
    private bool? _canDrag;
    private bool? _canDrop;
    private bool? _canDockAsDocument;

    public CapabilityPolicyEditor(string title, DockCapabilityPolicy policy)
    {
        Title = title;
        _policy = policy;
        _canClose = policy.CanClose;
        _canPin = policy.CanPin;
        _canFloat = policy.CanFloat;
        _canDrag = policy.CanDrag;
        _canDrop = policy.CanDrop;
        _canDockAsDocument = policy.CanDockAsDocument;
    }

    public string Title { get; }

    public DockCapabilityPolicy Policy => _policy;

    public bool? CanClose
    {
        get => _canClose;
        set
        {
            if (_canClose == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _canClose, value);
            _policy.CanClose = value;
        }
    }

    public bool? CanPin
    {
        get => _canPin;
        set
        {
            if (_canPin == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _canPin, value);
            _policy.CanPin = value;
        }
    }

    public bool? CanFloat
    {
        get => _canFloat;
        set
        {
            if (_canFloat == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _canFloat, value);
            _policy.CanFloat = value;
        }
    }

    public bool? CanDrag
    {
        get => _canDrag;
        set
        {
            if (_canDrag == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _canDrag, value);
            _policy.CanDrag = value;
        }
    }

    public bool? CanDrop
    {
        get => _canDrop;
        set
        {
            if (_canDrop == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _canDrop, value);
            _policy.CanDrop = value;
        }
    }

    public bool? CanDockAsDocument
    {
        get => _canDockAsDocument;
        set
        {
            if (_canDockAsDocument == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _canDockAsDocument, value);
            _policy.CanDockAsDocument = value;
        }
    }
}
