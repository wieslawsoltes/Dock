using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DockReactiveUIRoutingSample.ViewModels.Documents;

namespace DockReactiveUIRoutingSample.Views.Documents;

public partial class DocumentEditorView : ReactiveUserControl<DocumentEditorViewModel>
{
    public DocumentEditorView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}