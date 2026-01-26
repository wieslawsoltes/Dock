using System.Collections.ObjectModel;
using Dock.Model.ReactiveUI.Controls;
using DockFigmaSample.Models;

namespace DockFigmaSample.ViewModels.Tools;

public class ToolbarToolViewModel : Tool
{
    public ObservableCollection<ToolItem> Tools { get; } = SampleData.CreateToolbarTools();
}
