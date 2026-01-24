using System.Collections.ObjectModel;
using Dock.Model.ReactiveUI.Controls;
using DockFigmaSample.Models;

namespace DockFigmaSample.ViewModels.Tools;

public class LayersToolViewModel : Tool
{
    public ObservableCollection<LayerItem> Layers { get; } = SampleData.CreateLayers();
}
