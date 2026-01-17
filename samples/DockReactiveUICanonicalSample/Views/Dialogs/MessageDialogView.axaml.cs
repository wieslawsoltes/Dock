using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Dialogs;
using ReactiveUI.Avalonia;

namespace DockReactiveUICanonicalSample.Views.Dialogs;

public partial class MessageDialogView : ReactiveUserControl<MessageDialogViewModel>
{
    public MessageDialogView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
