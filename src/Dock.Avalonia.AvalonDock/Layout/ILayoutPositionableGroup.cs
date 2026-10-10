using Avalonia.Controls;

namespace Xceed.Wpf.AvalonDock.Layout;

internal interface ILayoutPositionableGroup
{
    GridLength DockWidth { get; set; }
    GridLength DockHeight { get; set; }
    double DockMinWidth { get; set; }
    double DockMinHeight { get; set; }
    double FloatingWidth { get; set; }
    double FloatingHeight { get; set; }
    double FloatingLeft { get; set; }
    double FloatingTop { get; set; }
    bool IsMaximized { get; set; }
    bool AllowDuplicateContent { get; set; }
    bool CanRepositionItems { get; set; }
    void ApplyDockProportion(double proportion, double availableExtent, bool horizontal);
}
