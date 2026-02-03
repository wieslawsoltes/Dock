using System;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using Dock.Settings;
using DockReactiveUIWindowRelationsSample.ViewModels.Documents;
using ReactiveUI;

namespace DockReactiveUIWindowRelationsSample.ViewModels.Tools;

public partial class WindowCasesToolViewModel : Tool
{
    private readonly IFactory _factory;
    private readonly IRootDock _root;
    private readonly IToolDock _stagingToolDock;
    private readonly IDocumentDock _documentDock;
    private int _toolCounter = 1;
    private int _documentCounter = 1;
    private IDockWindow? _parentWindow;

    private string? _status;

    public string? Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public ICommand OpenDefaultTool { get; }
    public ICommand OpenDockableOwnedTool { get; }
    public ICommand OpenNoOwnerTool { get; }
    public ICommand OpenParentOwnedTool { get; }
    public ICommand OpenRootOwnedTool { get; }
    public ICommand OpenModalRootTool { get; }
    public ICommand OpenModalNoOwnerTool { get; }
    public ICommand OpenNoTaskbarTool { get; }
    public ICommand OpenDefaultDocument { get; }
    public ICommand OpenRootOwnedDocument { get; }
    public ICommand SetOwnerPolicyDefault { get; }
    public ICommand SetOwnerPolicyAlwaysOwned { get; }
    public ICommand SetOwnerPolicyNeverOwned { get; }
    public ICommand SetHostModeDefault { get; }
    public ICommand SetHostModeNative { get; }
    public ICommand SetHostModeManaged { get; }

    public WindowCasesToolViewModel(
        IFactory factory,
        IRootDock root,
        IToolDock stagingToolDock,
        IDocumentDock documentDock)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _root = root ?? throw new ArgumentNullException(nameof(root));
        _stagingToolDock = stagingToolDock ?? throw new ArgumentNullException(nameof(stagingToolDock));
        _documentDock = documentDock ?? throw new ArgumentNullException(nameof(documentDock));

        OpenDefaultTool = ReactiveCommand.Create(() =>
            OpenToolCase("Default owner", new DockWindowOptions { OwnerMode = DockWindowOwnerMode.Default }));

        OpenDockableOwnedTool = ReactiveCommand.Create(() =>
            OpenToolCase("Dockable owner", new DockWindowOptions { OwnerMode = DockWindowOwnerMode.DockableWindow }));

        OpenNoOwnerTool = ReactiveCommand.Create(() =>
            OpenToolCase("No owner", new DockWindowOptions { OwnerMode = DockWindowOwnerMode.None }));

        OpenParentOwnedTool = ReactiveCommand.Create(OpenParentOwnedToolCase);

        OpenRootOwnedTool = ReactiveCommand.Create(() =>
            OpenToolCase("Root owner", new DockWindowOptions { OwnerMode = DockWindowOwnerMode.RootWindow }));

        OpenModalRootTool = ReactiveCommand.Create(() =>
            OpenToolCase("Modal (root owner)", new DockWindowOptions
            {
                OwnerMode = DockWindowOwnerMode.RootWindow,
                IsModal = true
            }));

        OpenModalNoOwnerTool = ReactiveCommand.Create(() =>
            OpenToolCase("Modal (no owner)", new DockWindowOptions
            {
                OwnerMode = DockWindowOwnerMode.None,
                IsModal = true
            }));

        OpenNoTaskbarTool = ReactiveCommand.Create(() =>
            OpenToolCase("No taskbar", new DockWindowOptions
            {
                OwnerMode = DockWindowOwnerMode.Default,
                ShowInTaskbar = false
            }));

        OpenDefaultDocument = ReactiveCommand.Create(() =>
            OpenDocumentCase("Document (default)", new DockWindowOptions
            {
                OwnerMode = DockWindowOwnerMode.Default
            }));

        OpenRootOwnedDocument = ReactiveCommand.Create(() =>
            OpenDocumentCase("Document (root owner)", new DockWindowOptions
            {
                OwnerMode = DockWindowOwnerMode.RootWindow
            }));

        SetOwnerPolicyDefault = ReactiveCommand.Create(() => SetOwnerPolicy(DockFloatingWindowOwnerPolicy.Default));
        SetOwnerPolicyAlwaysOwned = ReactiveCommand.Create(() => SetOwnerPolicy(DockFloatingWindowOwnerPolicy.AlwaysOwned));
        SetOwnerPolicyNeverOwned = ReactiveCommand.Create(() => SetOwnerPolicy(DockFloatingWindowOwnerPolicy.NeverOwned));

        SetHostModeDefault = ReactiveCommand.Create(() => SetHostMode(DockFloatingWindowHostMode.Default));
        SetHostModeNative = ReactiveCommand.Create(() => SetHostMode(DockFloatingWindowHostMode.Native));
        SetHostModeManaged = ReactiveCommand.Create(() => SetHostMode(DockFloatingWindowHostMode.Managed));
    }

    private void OpenParentOwnedToolCase()
    {
        var parentWindow = EnsureParentWindow();
        if (parentWindow is null)
        {
            Status = "Failed to locate a parent window.";
            return;
        }

        OpenToolCase("Parent owner", new DockWindowOptions
        {
            OwnerMode = DockWindowOwnerMode.ParentWindow,
            ParentWindow = parentWindow
        });
    }

    private void OpenToolCase(string title, DockWindowOptions options)
    {
        var tool = new CaseToolViewModel
        {
            Id = $"ToolCase{_toolCounter++:D2}",
            Title = title,
            Description = "Generated for window case testing.",
            CanClose = true
        };

        _factory.AddDockable(_stagingToolDock, tool);
        _factory.FloatDockable(tool, options);

        Status = $"Opened tool window: {title}.";
    }

    private void OpenDocumentCase(string title, DockWindowOptions options)
    {
        var document = new CaseDocumentViewModel
        {
            Id = $"DocCase{_documentCounter++:D2}",
            Title = title,
            Content = "Generated document for window case testing.",
            CanClose = true
        };

        _factory.AddDockable(_documentDock, document);
        _factory.FloatDockable(document, options);

        Status = $"Opened document window: {title}.";
    }

    private void SetOwnerPolicy(DockFloatingWindowOwnerPolicy policy)
    {
        DockSettings.FloatingWindowOwnerPolicy = policy;
        Status = $"Owner policy set to {policy}.";
    }

    private void SetHostMode(DockFloatingWindowHostMode mode)
    {
        DockSettings.FloatingWindowHostMode = mode;
        Status = $"Host mode set to {mode}. New windows will use this mode.";
    }

    private IDockWindow? EnsureParentWindow()
    {
        if (_parentWindow is not null && _root.Windows?.Contains(_parentWindow) == true)
        {
            return _parentWindow;
        }

        var tool = new CaseToolViewModel
        {
            Id = $"Parent{_toolCounter++:D2}",
            Title = "Parent window host",
            Description = "Parent window host for owner chain testing.",
            CanClose = true
        };

        _factory.AddDockable(_stagingToolDock, tool);
        _factory.FloatDockable(tool, new DockWindowOptions
        {
            OwnerMode = DockWindowOwnerMode.None
        });

        _parentWindow = FindWindowForDockable(tool);
        return _parentWindow;
    }

    private IDockWindow? FindWindowForDockable(IDockable dockable)
    {
        if (_root.Windows is null)
        {
            return null;
        }

        foreach (var window in _root.Windows)
        {
            if (window.Layout is not IDock layout)
            {
                continue;
            }

            if (ReferenceEquals(layout, dockable))
            {
                return window;
            }

            if (_factory.FindDockable(layout, candidate => ReferenceEquals(candidate, dockable)) is not null)
            {
                return window;
            }
        }

        return null;
    }
}
