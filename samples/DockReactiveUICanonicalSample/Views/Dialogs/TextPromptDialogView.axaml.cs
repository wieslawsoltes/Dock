using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Dialogs;
using Dock.Model.ReactiveUI.Services.Avalonia.Controls;

namespace DockReactiveUICanonicalSample.Views.Dialogs;

public partial class TextPromptDialogView : DockReactiveUserControl<TextPromptDialogViewModel>
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
