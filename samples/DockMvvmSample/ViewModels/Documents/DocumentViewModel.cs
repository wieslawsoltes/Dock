using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.CommandBars;
using Dock.Model.Mvvm.Controls;

namespace DockMvvmSample.ViewModels.Documents;

public class DocumentViewModel : Document, IDockCommandBarProvider
{
    private int _renameCounter;
    private readonly RelayCommand _toggleModifiedCommand;
    private readonly RelayCommand _renameCommand;
    private readonly RelayCommand _closeCommand;

    public DocumentViewModel()
    {
        _toggleModifiedCommand = new RelayCommand(ToggleModified);
        _renameCommand = new RelayCommand(RenameDocument);
        _closeCommand = new RelayCommand(CloseDocument);
    }

    public event EventHandler? CommandBarsChanged;

    public IReadOnlyList<DockCommandBarDefinition> GetCommandBars()
    {
        var displayTitle = IsModified ? $"{Title}*" : Title;

        var menuItems = new List<DockCommandBarItem>
        {
            new DockCommandBarItem("_Document")
            {
                Items = new List<DockCommandBarItem>
                {
                    new DockCommandBarItem($"Active: {displayTitle}") { Order = 0 },
                    new DockCommandBarItem("_Toggle Modified") { Command = _toggleModifiedCommand, Order = 1 },
                    new DockCommandBarItem("_Rename") { Command = _renameCommand, Order = 2 },
                    new DockCommandBarItem(null) { IsSeparator = true, Order = 3 },
                    new DockCommandBarItem("_Close") { Command = _closeCommand, Order = 4 }
                }
            }
        };

        var toolItems = new List<DockCommandBarItem>
        {
            new DockCommandBarItem("Toggle Modified") { Command = _toggleModifiedCommand, Order = 0 },
            new DockCommandBarItem("Rename") { Command = _renameCommand, Order = 1 },
            new DockCommandBarItem("Close") { Command = _closeCommand, Order = 2 }
        };

        return new List<DockCommandBarDefinition>
        {
            new DockCommandBarDefinition("DocumentMenu", DockCommandBarKind.Menu)
            {
                Order = 0,
                Items = menuItems
            },
            new DockCommandBarDefinition("DocumentToolBar", DockCommandBarKind.ToolBar)
            {
                Order = 1,
                Items = toolItems
            }
        };
    }

    private void ToggleModified()
    {
        IsModified = !IsModified;
        RaiseCommandBarsChanged();
    }

    private void RenameDocument()
    {
        _renameCounter++;
        Title = $"{Id} ({_renameCounter})";
        RaiseCommandBarsChanged();
    }

    private void CloseDocument()
    {
        if (!CanClose)
        {
            return;
        }

        Factory?.CloseDockable(this);
    }

    private void RaiseCommandBarsChanged()
    {
        CommandBarsChanged?.Invoke(this, EventArgs.Empty);
    }
}
