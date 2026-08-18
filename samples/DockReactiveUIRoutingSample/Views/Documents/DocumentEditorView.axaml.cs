using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia.Reactive;
using DockReactiveUIRoutingSample.ViewModels.Documents;

namespace DockReactiveUIRoutingSample.Views.Documents;

public partial class DocumentEditorView : ReactiveUserControl<DocumentEditorViewModel>
{
    public DocumentEditorView()
    {
        InitializeComponent();
    }
}
