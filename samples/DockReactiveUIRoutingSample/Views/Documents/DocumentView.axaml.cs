using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DockReactiveUIRoutingSample.ViewModels.Documents;

namespace DockReactiveUIRoutingSample.Views.Documents;

public partial class DocumentView : ReactiveUserControl<DocumentViewModel>
{
    public DocumentView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
