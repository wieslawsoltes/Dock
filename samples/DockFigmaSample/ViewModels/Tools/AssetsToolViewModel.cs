using System.Collections.ObjectModel;
using Dock.Model.ReactiveUI.Controls;
using DockFigmaSample.Models;

namespace DockFigmaSample.ViewModels.Tools;

public class AssetsToolViewModel : Tool
{
    public ObservableCollection<AssetSwatch> Swatches { get; } = SampleData.CreateSwatches();
    public ObservableCollection<ComponentItem> Components { get; } = SampleData.CreateComponents();
}
