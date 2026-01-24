using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Dock.Model.Controls;
using Dock.Model.Core;
using DockReactiveUIRiderSample.Services;
using DockReactiveUIRiderSample.ViewModels.Documents;
using ReactiveUI;

namespace DockReactiveUIRiderSample.ViewModels;

[RequiresUnreferencedCode("Requires unreferenced code for RaiseAndSetIfChanged.")]
[RequiresDynamicCode("Requires unreferenced code for RaiseAndSetIfChanged.")]
public class MainWindowViewModel : ReactiveObject
{
    private readonly DockFactory _factory;
    private readonly SolutionLoader _solutionLoader;
    private IRootDock? _layout;
    private string _statusText;
    private string _solutionName;
    private string _activeDocumentStatus;
    private EditorDocumentViewModel? _activeDocument;
    private IDisposable? _caretSubscription;

    public MainWindowViewModel()
    {
        _factory = new DockFactory();
        _solutionLoader = new SolutionLoader();
        _statusText = "Ready";
        _solutionName = "No solution";
        _activeDocumentStatus = "No document";

        var layout = _factory.CreateLayout();
        if (layout is not null)
        {
            _factory.InitLayout(layout);
        }

        Layout = layout;

        if (_factory.DocumentDock is Dock.Model.ReactiveUI.Controls.DocumentDock documentDock)
        {
            documentDock.WhenAnyValue(x => x.ActiveDockable)
                .Subscribe(dockable => SetActiveDocument(dockable as EditorDocumentViewModel));
        }

        _factory.SolutionExplorer.WhenAnyValue(x => x.SelectedItem)
            .Subscribe(item => _factory.PropertiesTool.UpdateSelection(item));

        var canSave = this.WhenAnyValue(x => x.ActiveDocument)
            .Select(document => document is not null);

        SaveFileCommand = ReactiveCommand.Create(SaveActiveFile, canSave);
        SaveAllCommand = ReactiveCommand.Create(SaveAllFiles);
        CloseSolutionCommand = ReactiveCommand.Create(CloseSolution);
        ExitCommand = ReactiveCommand.Create(Exit);
    }

    public IRootDock? Layout
    {
        get => _layout;
        set => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    public string SolutionName
    {
        get => _solutionName;
        set => this.RaiseAndSetIfChanged(ref _solutionName, value);
    }

    public string ActiveDocumentStatus
    {
        get => _activeDocumentStatus;
        set => this.RaiseAndSetIfChanged(ref _activeDocumentStatus, value);
    }

    public EditorDocumentViewModel? ActiveDocument
    {
        get => _activeDocument;
        private set => this.RaiseAndSetIfChanged(ref _activeDocument, value);
    }

    public ReactiveCommand<Unit, Unit> SaveFileCommand { get; }

    public ReactiveCommand<Unit, Unit> SaveAllCommand { get; }

    public ReactiveCommand<Unit, Unit> CloseSolutionCommand { get; }

    public ReactiveCommand<Unit, Unit> ExitCommand { get; }

    public async Task LoadSolutionAsync(string solutionPath)
    {
        StatusText = "Loading solution...";
        SolutionName = Path.GetFileName(solutionPath);

        try
        {
            var result = await _solutionLoader.LoadAsync(solutionPath, CancellationToken.None);
            if (result.Solution is null)
            {
                StatusText = "Failed to load solution.";
                return;
            }

            _factory.SolutionExplorer.LoadSolution(result.Solution);
            _factory.ProblemsTool.UpdateDiagnostics(result.Diagnostics);

            var projectCount = result.Solution.Projects.Count();
            StatusText = $"Loaded {projectCount} projects";
        }
        catch (Exception ex)
        {
            StatusText = $"Solution load failed: {ex.Message}";
        }
    }

    public void CloseLayout()
    {
        if (Layout is IDock dock && dock.Close.CanExecute(null))
        {
            dock.Close.Execute(null);
        }
    }

    private void SaveActiveFile()
    {
        if (ActiveDocument is null)
        {
            return;
        }

        ActiveDocument.Save();
        StatusText = $"Saved {ActiveDocument.Title}";
    }

    private void SaveAllFiles()
    {
        if (_factory.DocumentDock?.VisibleDockables is null)
        {
            return;
        }

        foreach (var document in _factory.DocumentDock.VisibleDockables.OfType<EditorDocumentViewModel>())
        {
            if (document.IsModified)
            {
                document.Save();
            }
        }

        StatusText = "Saved all documents";
    }

    private void CloseSolution()
    {
        _factory.SolutionExplorer.Clear();
        _factory.PropertiesTool.UpdateSelection(null);
        _factory.ProblemsTool.Clear();
        _factory.CloseAllDocuments();
        _solutionLoader.Clear();
        SetActiveDocument(null);

        SolutionName = "No solution";
        StatusText = "Ready";
    }

    private void Exit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.Shutdown();
        }
    }

    private void SetActiveDocument(EditorDocumentViewModel? document)
    {
        _caretSubscription?.Dispose();
        ActiveDocument = document;
        UpdateActiveDocumentStatus();

        if (document is null)
        {
            return;
        }

        _caretSubscription = document.WhenAnyValue(x => x.CaretLine, x => x.CaretColumn)
            .Subscribe(_ => UpdateActiveDocumentStatus());
    }

    private void UpdateActiveDocumentStatus()
    {
        if (ActiveDocument is null)
        {
            ActiveDocumentStatus = "No document";
            return;
        }

        ActiveDocumentStatus = $"{ActiveDocument.Title}  Ln {ActiveDocument.CaretLine}, Col {ActiveDocument.CaretColumn}";
    }
}
