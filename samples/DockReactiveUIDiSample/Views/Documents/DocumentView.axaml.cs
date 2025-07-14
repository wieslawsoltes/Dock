using Avalonia.Markup.Xaml;
using DockReactiveUIDiSample.ViewModels.Documents;
using Avalonia.ReactiveUI;

namespace DockReactiveUIDiSample.Views.Documents;

public partial class DocumentView : ReactiveUserControl<DocumentViewModel>
{
    public DocumentView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

