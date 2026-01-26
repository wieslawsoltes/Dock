using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dock.Model.ReactiveUI.Controls;
using DockReactiveUIRiderSample.Models;
using Microsoft.CodeAnalysis;

namespace DockReactiveUIRiderSample.ViewModels.Tools;

public class ProblemsToolViewModel : Tool
{
    public ProblemsToolViewModel()
    {
        Id = "Problems";
        Title = "Problems";
    }

    public ObservableCollection<ProblemItemViewModel> Diagnostics { get; } = new();

    public void UpdateDiagnostics(IEnumerable<WorkspaceDiagnostic> diagnostics)
    {
        Diagnostics.Clear();
        foreach (var diagnostic in diagnostics)
        {
            var severity = diagnostic.Kind == WorkspaceDiagnosticKind.Failure ? "Error" : "Warning";
            Diagnostics.Add(new ProblemItemViewModel(severity, diagnostic.Message));
        }
    }

    public void Clear()
    {
        Diagnostics.Clear();
    }
}
