using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DockReactiveUIRoutingSample.ViewModels.Documents;

namespace DockReactiveUIRoutingSample.Views.Documents;

public partial class DocumentHomeView : ReactiveUserControl<DocumentHomeViewModel>
{
    public DocumentHomeView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
