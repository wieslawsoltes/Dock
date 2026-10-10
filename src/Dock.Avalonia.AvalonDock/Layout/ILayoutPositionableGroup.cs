using Avalonia.Controls;

namespace AvalonDock.Layout;

internal interface ILayoutPositionableGroup : ILayoutElementForFloatingWindow
{
    GridLength DockWidth { get; set; }
    GridLength DockHeight { get; set; }
    double DockMinWidth { get; set; }
    double DockMinHeight { get; set; }
    bool AllowDuplicateContent { get; set; }
    bool CanRepositionItems { get; set; }
    void ApplyDockProportion(double proportion, double availableExtent, bool horizontal);
}
