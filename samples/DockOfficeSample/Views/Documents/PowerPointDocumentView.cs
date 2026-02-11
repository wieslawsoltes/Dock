using DockOfficeSample.ViewModels.Documents;
using Avalonia.ReactiveUI;

namespace DockOfficeSample.Views.Documents;

public class PowerPointDocumentView : ReactiveUserControl<PowerPointDocumentViewModel>
{
    public PowerPointDocumentView()
    {
        Content = new OfficeDocumentView();
    }
}
