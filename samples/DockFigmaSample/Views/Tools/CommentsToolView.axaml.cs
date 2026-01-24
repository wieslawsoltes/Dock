using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Tools;
using ReactiveUI.Avalonia;

namespace DockFigmaSample.Views.Tools;

public partial class CommentsToolView : ReactiveUserControl<CommentsToolViewModel>
{
    public CommentsToolView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
