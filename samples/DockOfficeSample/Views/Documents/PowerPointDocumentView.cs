using DockOfficeSample.ViewModels.Documents;
using ReactiveUI.Avalonia.Reactive;

namespace DockOfficeSample.Views.Documents;

public class PowerPointDocumentView : ReactiveUserControl<PowerPointDocumentViewModel>
{
    public PowerPointDocumentView()
    {
        Content = new OfficeDocumentView();
    }
}
