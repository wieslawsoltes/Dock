using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Dialogs;
using ReactiveUI.Avalonia;

namespace DockReactiveUICanonicalSample.Views.Dialogs;

public partial class TextPromptDialogView : ReactiveUserControl<TextPromptDialogViewModel>
{
    public TextPromptDialogView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
