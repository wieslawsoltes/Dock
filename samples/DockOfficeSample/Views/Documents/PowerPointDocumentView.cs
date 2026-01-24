using DockOfficeSample.ViewModels.Documents;
using ReactiveUI.Avalonia;

namespace DockOfficeSample.Views.Documents;

public class PowerPointDocumentView : ReactiveUserControl<PowerPointDocumentViewModel>
{
    public PowerPointDocumentView()
    {
        Content = new OfficeDocumentView();
    }
}
