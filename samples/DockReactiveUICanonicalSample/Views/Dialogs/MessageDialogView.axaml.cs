using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Dialogs;

namespace DockReactiveUICanonicalSample.Views.Dialogs;

public partial class MessageDialogView : DockReactiveUserControl<MessageDialogViewModel>
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
