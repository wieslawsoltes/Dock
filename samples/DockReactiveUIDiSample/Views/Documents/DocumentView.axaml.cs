using Avalonia.Markup.Xaml;
using DockReactiveUIDiSample.ViewModels.Documents;
using ReactiveUI.Avalonia.Reactive;

namespace DockReactiveUIDiSample.Views.Documents;

public partial class DocumentView : ReactiveUserControl<DocumentViewModel>
{
    public DocumentView()
    {
        InitializeComponent();
    }
}
