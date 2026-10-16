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
    void RestoreDockLengths(GridLength width, GridLength height);
    void RestoreDockMinimum(double value, bool horizontal);
    LayoutFloatingMetadata FloatingMetadata { get; set; }
    void ApplyDockProportion(double proportion, double availableExtent, bool horizontal);
}

internal readonly record struct LayoutFloatingMetadata(double Width, double Height, double Left, double Top, bool IsMaximized);
