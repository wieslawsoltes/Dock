using Dock.Model.ReactiveUI.Controls;
using DockReactiveUIRiderSample.Models;
using ReactiveUI;

namespace DockReactiveUIRiderSample.ViewModels.Tools;

public class PropertiesToolViewModel : Tool
{
    private string _itemName;
    private string _itemKind;
    private string _itemPath;

    public PropertiesToolViewModel()
    {
        Id = "Properties";
        Title = "Properties";
        _itemName = "No selection";
        _itemKind = string.Empty;
        _itemPath = string.Empty;
    }

    public string ItemName
    {
        get => _itemName;
        set => this.RaiseAndSetIfChanged(ref _itemName, value);
    }

    public string ItemKind
    {
        get => _itemKind;
        set => this.RaiseAndSetIfChanged(ref _itemKind, value);
    }

    public string ItemPath
    {
        get => _itemPath;
        set => this.RaiseAndSetIfChanged(ref _itemPath, value);
    }

    public void UpdateSelection(SolutionItemViewModel? item)
    {
        if (item is null)
        {
            ItemName = "No selection";
            ItemKind = string.Empty;
            ItemPath = string.Empty;
            return;
        }

        ItemName = item.Name;
        ItemKind = item.Kind.ToString();
        ItemPath = item.Path ?? string.Empty;
    }
}
