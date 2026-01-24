using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;

namespace DockOfficeSample.Views.Documents;

public partial class OfficeDocumentView : ReactiveUserControl<object>
{
    public OfficeDocumentView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
