using System;
using System.Collections.ObjectModel;
using Dock.Model.ReactiveUI.Controls;
using DockReactiveUIRiderSample.Models;
using DockReactiveUIRiderSample.Services;
using Microsoft.CodeAnalysis;
using ReactiveUI;

namespace DockReactiveUIRiderSample.ViewModels.Tools;

public class SolutionExplorerViewModel : Tool
{
    private readonly Action<string> _openDocument;
    private SolutionItemViewModel? _selectedItem;

    public SolutionExplorerViewModel(Action<string> openDocument)
    {
        _openDocument = openDocument;
        Id = "SolutionExplorer";
        Title = "Solution";
    }

    public ObservableCollection<SolutionItemViewModel> Items { get; } = new();

    public SolutionItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    public void LoadSolution(Solution solution)
    {
        Items.Clear();
        Items.Add(SolutionTreeBuilder.Build(solution));
    }

    public void Clear()
    {
        Items.Clear();
        SelectedItem = null;
    }

    public void OpenSelected()
    {
        if (SelectedItem?.Kind != SolutionItemKind.Document || string.IsNullOrWhiteSpace(SelectedItem.Path))
        {
            return;
        }

        _openDocument(SelectedItem.Path);
    }
}
