using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace DockOfficeSample.Views.Documents;

public partial class OfficeDocumentView : ReactiveUserControl<object>
{
    public OfficeDocumentView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
