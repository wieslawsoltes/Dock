using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DockReactiveUIRoutingSample.ViewModels.Documents;

namespace DockReactiveUIRoutingSample.Views.Documents;

public partial class DocumentDetailView : ReactiveUserControl<DocumentDetailViewModel>
{
    public DocumentDetailView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
