using DockOfficeSample.ViewModels.Documents;
using ReactiveUI.Avalonia.Reactive;

namespace DockOfficeSample.Views.Documents;

public class ExcelDocumentView : ReactiveUserControl<ExcelDocumentViewModel>
{
    public ExcelDocumentView()
    {
        Content = new OfficeDocumentView();
    }
}
